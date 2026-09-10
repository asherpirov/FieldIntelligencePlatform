using CSharpConsumer.Models;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging; 

namespace CSharpConsumer.Services;

public class ElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<ElasticsearchService> _logger; 

    public ElasticsearchService(string uri, string indexName, ILogger<ElasticsearchService> logger)
    {
        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(indexName);
        _client = new ElasticsearchClient(settings);
        _indexName = indexName;
        _logger = logger;
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
                _logger.LogInformation("Index '{IndexName}' created successfully with mappings.", _indexName);
            }
            else
            {
                _logger.LogError("Failed to communicate with Elasticsearch or external component. Index creation error: {Error}", createResponse.DebugInformation);
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
            _logger.LogInformation("Report {ReportId} saved successfully", report.ReportId);
        }
        else if (response.ElasticsearchServerError?.Status == 409)
        {
            _logger.LogWarning("Duplicate report rejected because reportId already exists: {ReportId}", report.ReportId);
        }
        else
        {
            _logger.LogError("Failed to communicate with Elasticsearch or external component. Details: {DebugInformation}", response.DebugInformation);
        }
    }
}