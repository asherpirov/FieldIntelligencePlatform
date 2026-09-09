using FieldIntelligenceApi.Models;

namespace FieldIntelligenceApi.Repositories;

public interface IReportRepository
{
    public Task<IEnumerable<ReportDocument>> SearchAsync(string? text);
}