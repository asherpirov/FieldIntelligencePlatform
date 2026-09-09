using CSharpConsumer.Models;
using Elastic.Clients.Elasticsearch;

namespace CSharpConsumer.Services;

public class ElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchService(string uri, string indexName)
    {
        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(indexName);
        _client = new ElasticsearchClient(settings);
        _indexName = indexName;
    }

    public async Task ProcessReportAsync(Report report)
    {
        var elasticDocument = new Dictionary<string, object>
        {
            {"reportId", report.ReportId },
            { "agentId", report.AgentId },
            { "unit", report.Unit },
            { "theater", report.Theater },
            { "sector", report.Sector },
            { "location", report.Location },
            { "reportType", report.ReportType },
            { "priority", report.Priority },
            { "sourceType", report.SourceType },
            { "message", report.Message },
            { "subjectId", report.SubjectId },
            { "subjectType", report.SubjectType },
            { "@timestamp", report.Timestamp },
            { "processedAt", DateTime.UtcNow }
        };

        var response = await _client.CreateAsync(elasticDocument, _indexName, report.ReportId);

        if (response.IsValidResponse)
        {
            Console.WriteLine($"[Elasticsearch] Report {report.ReportId} saved successfully");
        }
        else if (response.ElasticsearchServerError?.Status == 409)
        {
            Console.WriteLine($"[Log - Duplicate] Report {report.ReportId} already exists in Elasticsearch. Skipping");
        }
        else
        {
            Console.WriteLine($"[Elasticsearch Error] Failed to save {report.ReportId}: {response.DebugInformation}");
        }
    }
}