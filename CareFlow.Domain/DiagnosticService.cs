namespace CareFlow.Domain;

public class DiagnosticService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DailyCapacity { get; set; }
}