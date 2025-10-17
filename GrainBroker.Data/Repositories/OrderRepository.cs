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
            int page, int amount, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (amount <= 0) amount = 20;

        var query = _db.Orders.AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
            .Skip((page - 1) * amount)
            .Take(amount)
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
    public async Task<IReadOnlyList<GrainOrder>> GetLatestAsync(int amount, CancellationToken ct = default)
    {
        amount = Math.Clamp(amount, 1, 200);

        return await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .Take(amount)
            .ToListAsync(ct);
    }


}

