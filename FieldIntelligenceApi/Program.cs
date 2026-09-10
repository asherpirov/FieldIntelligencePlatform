using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Inference;
using FieldIntelligenceApi.Models;
using FieldIntelligenceApi.Repositories;

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var settings = config.GetSection("Elasticsearch").Get<ElasticsearchSettings>();
if (settings == null || string.IsNullOrWhiteSpace(settings.Uri))
{
   throw new InvalidOperationException("Elasticsearch configuration is missing in appsettings.json");
}

var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Uri)).DefaultIndex(settings.IndexName);
var esClient = new ElasticsearchClient(clientSettings);

builder.Services.AddSingleton(esClient);
builder.Services.AddSingleton<IReportRepository,ReportRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
