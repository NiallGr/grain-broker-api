namespace GrainBroker.Core.DTOs
{
    public record OrderDto
    {
        public int Id { get; init; }
        public DateTime OrderDate { get; init; }
        public Guid? PurchaseOrderId { get; init; }
        public Guid? CustomerId { get; init; }
        public string? CustomerLocation { get; init; }
        public decimal RequestedTons { get; init; }
        public decimal SuppliedTons { get; init; }
        public Guid? FulfilledById { get; init; }
        public string? FulfilledByLocation { get; init; }
        public decimal DeliveryCost { get; init; }
        public decimal FillRate { get; init; }
    }
}

