using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Confluent.Kafka;
using CSharpConsumer.Models;
using CSharpConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // חובה להוסיף בשביל ILogger

namespace CSharpConsumer;
class Program
{
    static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<Program>();
        var esLogger = loggerFactory.CreateLogger<ElasticsearchService>();

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var kafkaSettings = config.GetSection("Kafka").Get<KafkaSettings>();
        var esSettings = config.GetSection("Elasticsearch").Get<ElasticsearchSettings>();

        if (kafkaSettings == null)
        {
            logger.LogCritical("Failed to load Kafka settings.");
            return;
        }

        if (esSettings == null || string.IsNullOrWhiteSpace(esSettings.Uri))
        {
            logger.LogCritical("Failed to load Elasticsearch settings from appsettings.json."); 
            return;
        }

        var elasticService = new ElasticsearchService(esSettings.Uri, esSettings.IndexName, esLogger);
        await elasticService.InitializeIndexAsync();
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore,string>(consumerConfig).Build();
        consumer.Subscribe(kafkaSettings.TopicName);

        logger.LogInformation("Subscribed to topic: {TopicName}", kafkaSettings.TopicName);
        logger.LogInformation("Waiting for reports...");

        while (true)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(10));

                if (result == null || string.IsNullOrWhiteSpace(result.Message.Value))
                {
                    continue;
                }
                
                var message = result.Message.Value;
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                
                var report = JsonSerializer.Deserialize<Report>(message, jsonOptions);
                if (report == null)
                {
                    continue;
                }

                var validationContext = new ValidationContext(report);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(report, validationContext, validationResults, validateAllProperties: true);

                bool hasSubjectId = !string.IsNullOrWhiteSpace(report.SubjectId);
                bool hasSubjectType = !string.IsNullOrWhiteSpace(report.SubjectType);

                if (hasSubjectId != hasSubjectType)
                {
                    isValid = false;
                    validationResults.Add(new ValidationResult("subjectId and subjectType must appear together or be absent together."));
                }

                if (!isValid)
                {
                    string reasons = string.Join(" | ", validationResults.Select(v => v.ErrorMessage));
                    
                    logger.LogWarning("Report rejected due to validation. Reason: {Reasons}. ReportId: {ReportId}", reasons, report.ReportId);
                    continue;
                }
                
                logger.LogInformation("Valid Report Received! ID: {ReportId}, Type: {ReportType}, Location: {Location}", report.ReportId, report.ReportType, report.Location);
                await elasticService.ProcessReportAsync(report);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message from Kafka.");
            }
        }
    }
}