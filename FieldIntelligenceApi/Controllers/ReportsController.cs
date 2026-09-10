using FieldIntelligenceApi.Models;
using FieldIntelligenceApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FieldIntelligenceApi.Controllers;


[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _repository;

    public ReportsController(IReportRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ReportDocument>>> SearchAsync(
        [FromQuery] string? text,
        [FromQuery] string? theater,
        [FromQuery] string? sector,
        [FromQuery] string? location,
        [FromQuery] string[]? priorities,
        [FromQuery] string? reportType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        return Ok(await _repository.SearchAsync(text, theater, sector, location, priorities, reportType, from, to));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDocument>>> GetReportsWithlocationAsync(
       [FromQuery] string? theater,
       [FromQuery] string? sector,
       [FromQuery] string? location,
       [FromQuery] string[]? priorities,
       [FromQuery] DateTime? from,
       [FromQuery] DateTime? to)
    {
        return Ok(await _repository.GetReportsAsync(theater, sector, location, priorities, from, to));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<StatisticsResponseDto>> GetStatistics()
    {
        var statistics = await _repository.GetStatisticsAsync();
        return Ok(statistics);
    }

}