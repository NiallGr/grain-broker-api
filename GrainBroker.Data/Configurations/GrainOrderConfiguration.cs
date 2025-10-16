using GrainBroker.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrainBroker.Data.Configurations;
public class GrainOrderConfiguration : IEntityTypeConfiguration<GrainOrder>
{
    public void Configure(EntityTypeBuilder<GrainOrder> builder)
    {
        builder.ToTable("GrainOrders");

        builder.HasKey(x => x.Id)
               .HasName("PK_GrainOrders");

        builder.Property(x => x.PurchaseOrderId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.FulfilledById).IsRequired();

        builder.Property(x => x.RequestedTons).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SuppliedTons).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeliveryCost).HasColumnType("decimal(18,2)");

        builder.Property(x => x.CustomerLocation).HasMaxLength(120);
        builder.Property(x => x.FulfilledByLocation).HasMaxLength(120);

        builder.HasIndex(x => x.PurchaseOrderId)
               .HasDatabaseName("IX_GrainOrders_PurchaseOrderId");

    }
}