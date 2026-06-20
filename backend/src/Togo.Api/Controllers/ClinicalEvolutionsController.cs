using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Api.Security;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.ClinicalEvolutions.UseCases;
using Togo.Application.Tutors;

namespace Togo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/attendances/{attendanceId:long}/clinical-evolutions")]
public class ClinicalEvolutionsController : ControllerBase
{
    private readonly CreateClinicalEvolutionUseCase _createClinicalEvolutionUseCase;
    private readonly ListClinicalEvolutionsByAttendanceUseCase _listClinicalEvolutionsByAttendanceUseCase;
    private readonly ILogger<ClinicalEvolutionsController> _logger;

    public ClinicalEvolutionsController(
        CreateClinicalEvolutionUseCase createClinicalEvolutionUseCase,
        ListClinicalEvolutionsByAttendanceUseCase listClinicalEvolutionsByAttendanceUseCase,
        ILogger<ClinicalEvolutionsController> logger)
    {
        _createClinicalEvolutionUseCase = createClinicalEvolutionUseCase;
        _listClinicalEvolutionsByAttendanceUseCase = listClinicalEvolutionsByAttendanceUseCase;
        _logger = logger;
    }

    [Authorize(Policy = ClinicalEvolutionPolicies.Read)]
    [HttpGet]
    public async Task<IActionResult> ListByAttendance(long attendanceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clinical evolution list request received. AttendanceId: {AttendanceId}", attendanceId);

        var result = await _listClinicalEvolutionsByAttendanceUseCase.ExecuteAsync(attendanceId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize(Policy = ClinicalEvolutionPolicies.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        long attendanceId,
        [FromBody] CreateClinicalEvolutionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clinical evolution creation request received. AttendanceId: {AttendanceId}", attendanceId);

        var result = await _createClinicalEvolutionUseCase.ExecuteAsync(attendanceId, request, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ApplicationResult<T> result)
    {
        return result.Type switch
        {
            ApplicationResultType.Success => Ok(result.Data),
            ApplicationResultType.NotFound => NotFound(new { message = result.Error }),
            ApplicationResultType.ValidationError => BadRequest(new { message = result.Error }),
            ApplicationResultType.Conflict => Conflict(new { message = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Error ?? "Unexpected error." })
        };
    }
}
