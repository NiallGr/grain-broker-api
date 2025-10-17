using GrainBroker.Core.DTOs;

namespace GrainBroker.Core.Services
{
    public interface IOrderAnalysisService
    {
        Task<OrderInsightsDto> AnalyzeLatestAsync(CancellationToken ct = default);
    }
}
