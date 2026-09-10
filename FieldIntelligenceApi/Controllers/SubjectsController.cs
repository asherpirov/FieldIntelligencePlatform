using FieldIntelligenceApi.Models;
using FieldIntelligenceApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FieldIntelligenceApi.Controllers;


[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly IReportRepository _repository;

    public SubjectsController(IReportRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{subjectId}/reports")]
    public async Task<ActionResult<IEnumerable<ReportDocument>>> GetReportsWithSubjectAsync(string subjectId)
    {
        return Ok(await _repository.GetReportsWithSubjectAsync(subjectId));
    }


}