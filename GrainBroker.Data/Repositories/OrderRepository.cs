using GrainBroker.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrainBroker.Data.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly GrainBrokerDbContext _db;
    public OrderRepository(GrainBrokerDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<GrainOrder> items, CancellationToken ct = default)
        => await _db.Orders.AddRangeAsync(items, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public Task<GrainOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(IReadOnlyList<GrainOrder> items, int total)> ListAsync(
        int page, int pageSize, string? q, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(o =>
                o.PurchaseOrder.Contains(term) ||
                o.CustomerId.Contains(term) ||
                (o.CustomerLocation != null && o.CustomerLocation.Contains(term)) ||
                (o.FulfilledById != null && o.FulfilledById.Contains(term)) ||
                (o.FulfilledByLocation != null && o.FulfilledByLocation.Contains(term)));
        }

        if (from.HasValue) query = query.Where(o => o.OrderDate >= from.Value.Date);
        if (to.HasValue) query = query.Where(o => o.OrderDate < to.Value.Date.AddDays(1));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;
        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
