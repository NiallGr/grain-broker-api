using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrainBroker.Data.Entities;
public class GrainOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid CustomerId { get; set; }
    [MaxLength(220)] public string? CustomerLocation { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal RequestedTons { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal SuppliedTons { get; set; }

    public Guid? FulfilledById { get; set; }
    [MaxLength(220)] public string? FulfilledByLocation { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal DeliveryCost { get; set; }
}
