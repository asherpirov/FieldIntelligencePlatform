using Elastic.Clients.Elasticsearch;
using FieldIntelligenceApi.Models;

namespace FieldIntelligenceApi.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ElasticsearchClient _client;
    public ReportRepository(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<ReportDocument>> SearchAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<ReportDocument>();
        }

        var response = await _client.SearchAsync<ReportDocument>(s => s
            .Query(q => q
                 .Match(m => m
                    .Field(f => f.Message)
                    .Query(text)
                )
            )
        );

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to search in Elasticsearch: {response.DebugInformation}");
        }
        return response.Documents;

    }

}