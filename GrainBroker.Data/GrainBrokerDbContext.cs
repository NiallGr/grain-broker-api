using GrainBroker.Data.Entities;
using Microsoft.EntityFrameworkCore;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace GrainBroker.Data;
public class GrainBrokerDbContext : DbContext
{
    public GrainBrokerDbContext(DbContextOptions<GrainBrokerDbContext> options) : base(options) { }
    public Microsoft.EntityFrameworkCore.DbSet<GrainOrder> Orders => Set<GrainOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GrainBrokerDbContext).Assembly);

        modelBuilder.Entity<GrainOrder>()
            .HasIndex(x => x.PurchaseOrderId);
    }
}
