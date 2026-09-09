namespace CSharpConsumer.Models;

public class ElasticReportDocument
{
    public string ReportId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Theater { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime ProcessedAt { get; set; }
}