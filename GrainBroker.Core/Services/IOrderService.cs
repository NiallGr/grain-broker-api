using GrainBroker.Core.DTOs;

namespace GrainBroker.Core.Services;
public interface IOrderService
{
    // Import (row-tolerant; returns successes + per-row failures)
    Task<ImportResultDto> ImportCsvAsync(Stream csvStream, CancellationToken ct = default);

    // Queries
    Task<PagedResult<OrderDto>> ListAsync(int page, int pageSize, string? q, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<OrderDto?> GetAsync(int id, CancellationToken ct = default);

    // Deletes
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
