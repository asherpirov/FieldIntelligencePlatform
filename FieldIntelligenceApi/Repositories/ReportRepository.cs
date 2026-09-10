using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FieldIntelligenceApi.Models;

namespace FieldIntelligenceApi.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ElasticsearchClient _client;
    public ReportRepository(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<ReportDocument>> SearchAsync(
        string? text, string? theater, string? sector, string? location,
        string[]? priorities, string? reportType, DateTime? from, DateTime? to)
    {
        var mustQueries = new List<Action<QueryDescriptor<ReportDocument>>>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            mustQueries.Add(q => q.Match(t => t.Field(f => f.Message).Query(text)));
        }

        if (!string.IsNullOrWhiteSpace(theater))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Theater).Value(theater)));
        }

        if (!string.IsNullOrWhiteSpace(sector))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Sector).Value(sector)));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Location).Value(location)));
        }

        if (!string.IsNullOrWhiteSpace(reportType))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.ReportType).Value(reportType)));
        }

        if (priorities != null && priorities.Any())
        {
            mustQueries.Add(q => q.Terms(t => t
            .Field(f => f.Priority)
            .Term(new TermsQueryField(priorities.Select(p => FieldValue.String(p)).ToList()))
        ));
        }

        if (from.HasValue || to.HasValue)
        {
            mustQueries.Add(q => q.Range(r => r
            .DateRange(d =>
            {
                d.Field(f => f.Timestamp);
            
                if (from.HasValue)
                { 
                    d.Gte(from.Value);
                }
                if (to.HasValue)
                { 
                    d.Lte(to.Value);
                }
            })
        ));
    }
        var response = await _client.SearchAsync<ReportDocument>(s => s
        .Query(q => q
            .Bool(b => b
                .Must(mustQueries.ToArray())
                )
            )
        );
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to search in Elasticsearch: {response.DebugInformation}");
        }
        return response.Documents;
    }
    

    public async Task<IEnumerable<ReportDocument>> GetReportsWithSubjectAsync(string subjectId)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Enumerable.Empty<ReportDocument>();
        }

        var response = await _client.SearchAsync<ReportDocument>(s => s
        .Query(q => q
            .Term(t => t
                .Field(f => f.SubjectId)
                .Value(subjectId)
            )
        )
        .Sort(srt => srt
            .Field(f => f.Timestamp, fs => fs.Order(SortOrder.Asc))
        )
    );

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to search in Elasticsearch: {response.DebugInformation}");
        }
        return response.Documents;  
    }

    public async Task<IEnumerable<ReportDocument>> GetReportsAsync(
        string? theater, string? sector, string? location, 
        string[]? priorities, DateTime? from, DateTime? to)
    {
        var mustQueries = new List<Action<QueryDescriptor<ReportDocument>>>();

        if (!string.IsNullOrWhiteSpace(theater))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Theater).Value(theater)));
        }

        if (!string.IsNullOrWhiteSpace(sector))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Sector).Value(sector)));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Location).Value(location)));
        }

        if (priorities != null && priorities.Any())
        {
            mustQueries.Add(q => q.Terms(t => t
            .Field(f => f.Priority)
            .Term(new TermsQueryField(priorities.Select(p => FieldValue.String(p)).ToList()))
        ));
        }

        if (from.HasValue || to.HasValue)
        {
            mustQueries.Add(q => q.Range(r => r
            .DateRange(d =>
            {
                d.Field(f => f.Timestamp);
            
                if (from.HasValue)
                { 
                    d.Gte(from.Value);
                }
                if (to.HasValue)
                { 
                    d.Lte(to.Value);
                }
            })
        ));
    }  
        var response = await _client.SearchAsync<ReportDocument>(s => s
        .Query(q => q
            .Bool(b => b
                .Must(mustQueries.ToArray())
                )
            )
        );
  
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to search in Elasticsearch: {response.DebugInformation}");
        }
        return response.Documents;
    }

    public async Task<StatisticsResponseDto> GetStatisticsAsync()
    {
        var response = await _client.SearchAsync<ReportDocument>(s => s
            .Size(0)
            .Aggregations(a => a
                .Add("by_priority", agg => agg
                    .Terms(t => t.Field(f => f.Priority)))
                .Add("by_theater", agg => agg
                    .Terms(t => t.Field(f => f.Theater)))
                .Add("by_report_type", agg => agg
                    .Terms(t => t.Field(f => f.ReportType)))
            )
        );

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to search in Elasticsearch: {response.DebugInformation}");
        }

        var result = new StatisticsResponseDto();

        var priorityAgg =
            response.Aggregations.GetStringTerms("by_priority");

        if (priorityAgg != null)
        {
            foreach (var bucket in priorityAgg.Buckets)
            {
                result.ByPriority[bucket.Key.ToString()] = bucket.DocCount;
            }
        }

        var theaterAgg = response.Aggregations.GetStringTerms("by_theater");

        if (theaterAgg != null)
        {
            foreach (var bucket in theaterAgg.Buckets)
            {
                result.ByTheater[bucket.Key.ToString()] = bucket.DocCount;
            }
        }

        var reportTypeAgg = response.Aggregations.GetStringTerms("by_report_type");

        if (reportTypeAgg != null)
        {
            foreach (var bucket in reportTypeAgg.Buckets)
            {
                result.ByReportType[bucket.Key.ToString()] = bucket.DocCount;
            }
        }
        return result;
    }
}

       
