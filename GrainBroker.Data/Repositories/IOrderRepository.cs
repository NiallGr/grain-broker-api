using GrainBroker.Data.Entities;

namespace GrainBroker.Data.Repositories;
public interface IOrderRepository
{
    // Import
    Task AddRangeAsync(IEnumerable<GrainOrder> items, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // Queries
    Task<GrainOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<GrainOrder> items, int total)> ListAsync(
        int page, int pageSize, string? q, DateTime? from, DateTime? to, CancellationToken ct = default);

    // Deletes
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
