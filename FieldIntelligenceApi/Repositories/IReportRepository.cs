using FieldIntelligenceApi.Models;

namespace FieldIntelligenceApi.Repositories;

public interface IReportRepository
{
    public Task<IEnumerable<ReportDocument>> SearchAsync(string? text,
    string? theater, string? sector, string? location,
        string[]? priorities, string? reportType, DateTime? from, DateTime? to);
    public Task<IEnumerable<ReportDocument>> GetReportsWithSubjectAsync(string subjectId);

    public Task<IEnumerable<ReportDocument>> GetReportsAsync(
        string? theater, string? sector, string? location,
        string[]? priorities, DateTime? from, DateTime? to);

    public Task<StatisticsResponseDto> GetStatisticsAsync();

}