namespace GrainBroker.Core.DTOs;
public class ImportFailureDto
{
    public int Row { get; set; }                         // 1-based incl. header
    public string Reason { get; set; } = "";
    public Dictionary<string, string?> Raw { get; set; } = new();
}

public class ImportResultDto
{
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<ImportFailureDto> Failures { get; set; } = new();
}
