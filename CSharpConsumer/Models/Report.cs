using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CSharpConsumer.Models;
public class Report
{
    [Required]
    public string ReportId {get; set;} = string.Empty;
    [Required]
    public DateTime Timestamp {get;set;} = DateTime.UtcNow;

    [Required]
    public string AgentId {get; set;} = string.Empty;

    [Required]
    public string Unit {get;set;} = string.Empty;

    [Required]
    public string Theater {get; set;} = string.Empty;
    [Required]
    public string Sector {get; set;} = string.Empty;
    [Required]
    public string Location {get;set;} = string.Empty;
    [Required]
    [AllowedValues("Observation", "Movement", "Meeting", "Access","Communication", "Logistics", "Incident")]
    public string ReportType {get; set;} = string.Empty;
    [Required]
    [AllowedValues("Low", "Medium", "High", "Critical")]
    public string Priority {get; set;} = string.Empty;

    [Required]
    public string SourceType {get;set;} = string.Empty;

    [Required]
    public string Message {get; set;} = string.Empty;
    public string SubjectId {get; set;} = string.Empty;
    public string SubjectType {get;set;} = string.Empty;

}