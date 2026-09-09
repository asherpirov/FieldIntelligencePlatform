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

    public async Task InitializeIndexAsync()
    {
        var existsResponse = await _client.Indices.ExistsAsync(_indexName);
        
        if (!existsResponse.Exists)
        {
            var createResponse = await _client.Indices.CreateAsync(_indexName, c => c
                .Mappings(m => m
                    .Properties<ElasticReportDocument>(p => p
                        .Date(f => f.Timestamp)
                        .Date(f => f.ProcessedAt)
                        .Text(f => f.Message, t => t.Fields(fs => fs.Keyword("keyword")))
                        .Keyword(f => f.ReportId)
                        .Keyword(f => f.AgentId)
                        .Keyword(f => f.Unit)
                        .Keyword(f => f.Theater)
                        .Keyword(f => f.Sector)
                        .Keyword(f => f.Location)
                        .Keyword(f => f.ReportType)
                        .Keyword(f => f.Priority)
                        .Keyword(f => f.SourceType)
                        .Keyword(f => f.SubjectId)
                        .Keyword(f => f.SubjectType)
                    )
                )
            );

            if (createResponse.IsValidResponse)
            {
                Console.WriteLine($"[Elasticsearch] Index '{_indexName}' created successfully with mappings.");
            }
            else
            {
                Console.WriteLine($"[Elasticsearch Error] Failed to create index: {createResponse.DebugInformation}");
            }
        }
    }
    public async Task ProcessReportAsync(Report report)
    {
        var elasticDoc = new ElasticReportDocument
        {
            ReportId = report.ReportId,
            AgentId = report.AgentId,
            Unit = report.Unit,
            Theater = report.Theater,
            Sector = report.Sector,
            Location = report.Location,
            ReportType = report.ReportType,
            Priority = report.Priority,
            SourceType = report.SourceType,
            Message = report.Message,
            SubjectId = report.SubjectId,
            SubjectType = report.SubjectType,
            Timestamp = report.Timestamp,
            ProcessedAt = DateTime.UtcNow
        };

        var response = await _client.CreateAsync(elasticDoc, _indexName, report.ReportId);

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