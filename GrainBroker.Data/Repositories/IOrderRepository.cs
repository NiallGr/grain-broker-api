using GrainBroker.Data.Entities;

namespace GrainBroker.Data.Repositories;
public interface IOrderRepository
{
    Task AddRangeAsync(IEnumerable<GrainOrder> items, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<GrainOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<GrainOrder> items, int total)> ListAsync(int page, int amount, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<GrainOrder>> GetLatestAsync(int amount, CancellationToken ct = default);

}
