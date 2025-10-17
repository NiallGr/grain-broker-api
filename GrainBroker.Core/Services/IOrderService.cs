using GrainBroker.Core.DTOs;

namespace GrainBroker.Core.Services;
public interface IOrderService
{
Task<PagedResult<OrderDto>> ListAsync(int page, int amount, CancellationToken ct = default);
    Task<ImportResultDto> ImportCsvAsync(Stream csvStream, CancellationToken ct = default);
    Task<OrderDto?> GetAsync(int id, CancellationToken ct = default);

    // Deletes
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
