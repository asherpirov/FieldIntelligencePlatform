using FieldIntelligenceApi.Models;
using FieldIntelligenceApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FieldIntelligenceApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _repository;

    public ReportsController(IReportRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ReportDocument>>> SearchAsync(
        [FromQuery] string? text)
    {
        return Ok(await _repository.SearchAsync(text));
    }

}