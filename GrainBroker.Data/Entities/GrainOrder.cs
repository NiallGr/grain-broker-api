using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrainBroker.Data.Entities;
public class GrainOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    [MaxLength(80)] public string PurchaseOrder { get; set; } = string.Empty;
    [MaxLength(80)] public string CustomerId { get; set; } = string.Empty;
    [MaxLength(120)] public string? CustomerLocation { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal RequestedTons { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal SuppliedTons { get; set; }

    [MaxLength(80)] public string? FulfilledById { get; set; }
    [MaxLength(120)] public string? FulfilledByLocation { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal DeliveryCost { get; set; }
}
