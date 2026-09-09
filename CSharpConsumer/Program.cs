using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using Confluent.Kafka;
using CSharpConsumer.Models;
using CSharpConsumer.Services;
using Microsoft.Extensions.Configuration;

namespace CSharpConsumer;
class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

        var kafkaSettings = config.GetSection("Kafka").Get<KafkaSettings>();
        var esSettings = config.GetSection("Elasticsearch").Get<ElasticsearchSettings>();


        if (kafkaSettings == null)
        {
            Console.WriteLine("Failed to load Kafka settings.");
            return;
        }

        if (esSettings == null || string.IsNullOrWhiteSpace(esSettings.Uri))
        {
            Console.WriteLine("Failed to load Elasticsearch settings from appsettings.json."); 
            return;
        }

        var elasticService = new ElasticsearchService(esSettings.Uri, esSettings.IndexName);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore,string>(consumerConfig).Build();
        consumer.Subscribe(kafkaSettings.TopicName);

        Console.WriteLine($"Subscribed to topic: {kafkaSettings.TopicName}");
        Console.WriteLine("Waiting for reports...");

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
                    validationResults.Add(new ValidationResult("subjectId and subjectType must t appear together or be absent together."));
                }

                if (!isValid)
                {
                    string reasons = string.Join(" | ", validationResults.Select(v => v.ErrorMessage));
                    Console.WriteLine($"[Log - Rejected] Report {report.ReportId} rejected. Reasons: {reasons}");
                    continue;
                }
                Console.WriteLine($"[+] Valid Report Received! ID: {report.ReportId}, Type: {report.ReportType}, Location: {report.Location}");
                await elasticService.ProcessReportAsync(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log - Error] System Exception: {ex.Message}");
            }
        }

    }
}
