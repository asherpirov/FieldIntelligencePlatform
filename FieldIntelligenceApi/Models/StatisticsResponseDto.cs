namespace FieldIntelligenceApi.Models;

public class StatisticsResponseDto
{    
    public Dictionary<string, long> ByPriority { get; set; } = new();
    
    public Dictionary<string, long> ByReportType { get; set; } = new();
    
    public Dictionary<string, long> ByTheater { get; set; } = new();
}