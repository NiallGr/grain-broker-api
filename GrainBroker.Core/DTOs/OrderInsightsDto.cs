namespace GrainBroker.Core.DTOs;
public sealed class OrderInsightsDto
{
    public string Summary { get; set; } = string.Empty;
    public string[] KeyFindings { get; set; } = Array.Empty<string>();
    public decimal TotalRequestedTons { get; set; }
    public decimal TotalSuppliedTons { get; set; }
    public decimal AvgFillRate { get; set; }
    public decimal MedianDeliveryCost { get; set; }
    public decimal AvgDeliveryCost { get; set; }
}
