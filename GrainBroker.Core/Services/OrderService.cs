using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using GrainBroker.Core.DTOs;
using GrainBroker.Data.Entities;
using GrainBroker.Data.Repositories;
using System.Globalization;

namespace GrainBroker.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository repo, IMapper mapper)
        { _repo = repo; _mapper = mapper; }

        public async Task<ImportResultDto> ImportCsvAsync(Stream csvStream, CancellationToken ct = default)
        {
            var result = new ImportResultDto();

            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header?.Trim(),
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                DetectDelimiter = true
            };

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, cfg);

            if (!await csv.ReadAsync() || !csv.ReadHeader())
                throw new InvalidOperationException("CSV has no header row.");

            var headers = csv.HeaderRecord!.Select(h => h.Trim()).ToArray();
            string[] required = new[]
            {
                "Order Date", "Purchase Order", "Customer ID", "Customer Location",
                "Order Req Amt (Ton)", "Fullfilled By ID", "Fullfilled By Location",
                "Supplied Amt (Ton)", "Cost Of Delivery ($)"
            };
            var missing = required.Where(r => !headers.Contains(r)).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException("Missing columns: " + string.Join(", ", missing));

            static bool TryDec(string? s, out decimal d)
                => decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d);

            static bool TryDate(string? s, out DateTime d)
                => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d);

            var valid = new List<GrainOrder>();
            var rowIndex = 1;

            while (await csv.ReadAsync())
            {
                rowIndex++;

                string F(string name) => csv.GetField(name);

                string orderDate = F("Order Date");
                string purchaseOrder = F("Purchase Order");
                string customerId = F("Customer ID");
                string customerLocation = F("Customer Location");
                string requestedTons = F("Order Req Amt (Ton)");
                string suppliedTons = F("Supplied Amt (Ton)");
                string fulfilledById = F("Fullfilled By ID");
                string fulfilledByLocation = F("Fullfilled By Location");
                string deliveryCost = F("Cost Of Delivery ($)");

                var raw = new Dictionary<string, string?>
                {
                    ["Order Date"] = orderDate,
                    ["Purchase Order"] = purchaseOrder,
                    ["Customer ID"] = customerId,
                    ["Customer Location"] = customerLocation,
                    ["Order Req Amt (Ton)"] = requestedTons,
                    ["Supplied Amt (Ton)"] = suppliedTons,
                    ["Fullfilled By ID"] = fulfilledById,
                    ["Fullfilled By Location"] = fulfilledByLocation,
                    ["Cost Of Delivery ($)"] = deliveryCost
                };

                var reasons = new List<string>();
                if (!TryDate(orderDate, out var dt)) reasons.Add("Order Date invalid");
                if (string.IsNullOrWhiteSpace(purchaseOrder)) reasons.Add("Purchase Order required");
                if (string.IsNullOrWhiteSpace(customerId)) reasons.Add("Customer ID required");
                if (!TryDec(requestedTons, out var req)) reasons.Add("Order Req Amt (Ton) invalid");
                if (!TryDec(suppliedTons, out var sup)) reasons.Add("Supplied Amt (Ton) invalid");
                if (!TryDec(deliveryCost, out var cost)) reasons.Add("Cost Of Delivery ($) invalid");

                Guid purchaseOrderGuid = Guid.Empty;
                Guid customerGuid = Guid.Empty;
                Guid? fulfilledByGuid = null;
                Guid parsedFulfilled;

                if (reasons.Count == 0)
                {
                    if (!Guid.TryParse(purchaseOrder.Trim(), out purchaseOrderGuid))
                        reasons.Add("Purchase Order must be a valid GUID");
                    if (!Guid.TryParse(customerId.Trim(), out customerGuid))
                        reasons.Add("Customer ID must be a valid GUID");
                    if (!string.IsNullOrWhiteSpace(fulfilledById))
                    {
                        if (Guid.TryParse(fulfilledById.Trim(), out parsedFulfilled))
                            fulfilledByGuid = parsedFulfilled;
                        else
                            reasons.Add("Fullfilled By ID must be a valid GUID");
                    }

                    if (req < 0) reasons.Add("RequestedTons cannot be negative");
                    if (sup < 0) reasons.Add("SuppliedTons cannot be negative");
                    if (cost < 0) reasons.Add("DeliveryCost cannot be negative");
                }

                if (reasons.Count > 0)
                {
                    result.Failures.Add(new ImportFailureDto
                    {
                        Row = rowIndex,
                        Reason = string.Join("; ", reasons),
                        Raw = raw
                    });
                    continue;
                }

                valid.Add(new GrainOrder
                {
                    OrderDate = dt,
                    PurchaseOrderId = purchaseOrderGuid,
                    CustomerId = customerGuid,
                    CustomerLocation = string.IsNullOrWhiteSpace(customerLocation) ? null : customerLocation.Trim(),
                    RequestedTons = req,
                    SuppliedTons = sup,
                    FulfilledById = fulfilledByGuid,
                    FulfilledByLocation = string.IsNullOrWhiteSpace(fulfilledByLocation) ? null : fulfilledByLocation.Trim(),
                    DeliveryCost = cost
                });
            }

            if (valid.Count > 0)
            {
                await _repo.AddRangeAsync(valid, ct);
                await _repo.SaveChangesAsync(ct);
            }

            result.Imported = valid.Count;
            result.Failed = result.Failures.Count;
            return result;
        }

        public async Task<PagedResult<OrderDto>> ListAsync(int page, int amount, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            amount = Math.Clamp(amount, 1, 200);

            var (items, total) = await _repo.ListAsync(page, amount, ct);
            var dtos = _mapper.Map<IReadOnlyList<OrderDto>>(items);
            return new PagedResult<OrderDto>(page, amount, total, dtos);
        }


        public async Task<OrderDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            return entity is null ? null : _mapper.Map<OrderDto>(entity);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);
    }
}
