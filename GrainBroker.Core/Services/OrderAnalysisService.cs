using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GrainBroker.Core.DTOs;
using GrainBroker.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace GrainBroker.Core.Services
{
    public sealed class OrderAnalysisService : IOrderAnalysisService
    {
        private readonly IOrderRepository _repo;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        private const int RecordLimit = 200;
        private const string DefaultModel = "gpt-4.1-mini";
        private const double DefaultTemperature = 0.2;

        public OrderAnalysisService(IOrderRepository repo, IHttpClientFactory httpFactory, IConfiguration config)
        {
            _repo = repo;
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<OrderInsightsDto> AnalyzeLatestAsync(CancellationToken ct = default)
        {
            var key = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            var orders = await _repo.GetLatestAsync(RecordLimit, ct);
            var baseline = ComputeBaseline(orders);

            var compact = orders.Select(o => new
            {
                id = o.Id,
                date = o.OrderDate.ToString("yyyy-MM-dd"),
                po = o.PurchaseOrderId,
                cust = o.CustomerId,
                custLoc = o.CustomerLocation,
                req = o.RequestedTons,
                sup = o.SuppliedTons,
                fill = o.RequestedTons == 0 ? 0m : o.SuppliedTons / o.RequestedTons,
                by = o.FulfilledById,
                byLoc = o.FulfilledByLocation,
                cost = o.DeliveryCost
            });

            var dataJson = JsonSerializer.Serialize(compact);

            var systemPrompt =
                @"You are a data analyst for a grain brokerage. Analyze recent order records and summarize operational insights.";

            var userPrompt =
                $@"Analyze these {orders.Count} latest orders (JSON below). Find trends, anomalies, and opportunities.
                Focus on fill rate, delivery cost, and customer performance.
                Return ONLY valid JSON matching this structure (no markdown, no commentary):

                {{
                  ""Summary"": string,
                  ""KeyFindings"": string[],
                  ""TotalRequestedTons"": number,
                  ""TotalSuppliedTons"": number,
                  ""AvgFillRate"": number,
                  ""MedianDeliveryCost"": number,
                  ""AvgDeliveryCost"": number
                }}

                DATA:
                {dataJson}";

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var payload = new
            {
                model = DefaultModel,
                temperature = DefaultTemperature,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            try
            {
                using var res = await client.SendAsync(req, ct);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("Empty response from AI model.");

                var insights = JsonSerializer.Deserialize<OrderInsightsDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new OrderInsightsDto();

                // Backfill with baseline if model omitted any numbers
                insights.TotalRequestedTons = insights.TotalRequestedTons == 0 ? baseline.TotalRequestedTons : insights.TotalRequestedTons;
                insights.TotalSuppliedTons = insights.TotalSuppliedTons == 0 ? baseline.TotalSuppliedTons : insights.TotalSuppliedTons;
                insights.AvgFillRate = insights.AvgFillRate == 0 ? baseline.AvgFillRate : insights.AvgFillRate;
                insights.AvgDeliveryCost = insights.AvgDeliveryCost == 0 ? baseline.AvgDeliveryCost : insights.AvgDeliveryCost;
                insights.MedianDeliveryCost = insights.MedianDeliveryCost == 0 ? baseline.MedianDeliveryCost : insights.MedianDeliveryCost;

                if (string.IsNullOrWhiteSpace(insights.Summary))
                    insights.Summary = "Latest orders analyzed. See key findings below.";

                return insights;
            }
            catch
            {
                return new OrderInsightsDto
                {
                    Summary = "AI analysis unavailable. Returning baseline metrics only.",
                    KeyFindings = Array.Empty<string>(),
                    TotalRequestedTons = baseline.TotalRequestedTons,
                    TotalSuppliedTons = baseline.TotalSuppliedTons,
                    AvgFillRate = baseline.AvgFillRate,
                    AvgDeliveryCost = baseline.AvgDeliveryCost,
                    MedianDeliveryCost = baseline.MedianDeliveryCost
                };
            }
        }

        private static OrderInsightsDto ComputeBaseline(IReadOnlyList<Data.Entities.GrainOrder> orders)
        {
            if (orders.Count == 0) return new OrderInsightsDto();

            decimal totalReq = orders.Sum(o => o.RequestedTons);
            decimal totalSup = orders.Sum(o => o.SuppliedTons);

            var fillRates = orders.Select(o => o.RequestedTons == 0 ? 0m : o.SuppliedTons / o.RequestedTons).ToArray();
            var costs = orders.Select(o => o.DeliveryCost).OrderBy(x => x).ToArray();

            decimal avgFill = fillRates.Length == 0 ? 0 : fillRates.Average();
            decimal avgCost = costs.Length == 0 ? 0 : costs.Average();
            decimal medianCost = 0;
            if (costs.Length > 0)
            {
                int mid = costs.Length / 2;
                medianCost = (costs.Length % 2 == 0)
                    ? (costs[mid - 1] + costs[mid]) / 2m
                    : costs[mid];
            }

            return new OrderInsightsDto
            {
                TotalRequestedTons = totalReq,
                TotalSuppliedTons = totalSup,
                AvgFillRate = avgFill,
                AvgDeliveryCost = avgCost,
                MedianDeliveryCost = medianCost
            };
        }
    }
}
