namespace GrainBroker.Core.DTOs
{
    public record OrderDto(
        int Id,
        DateTime OrderDate,
        string PurchaseOrder,
        string CustomerId,
        string? CustomerLocation,
        decimal RequestedTons,
        decimal SuppliedTons,
        string? FulfilledById,
        string? FulfilledByLocation,
        decimal DeliveryCost,
        decimal FillRate // computed: SuppliedTons / RequestedTons
    );
}

