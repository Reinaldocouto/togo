using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Api.Security;
using Togo.Application.Prescriptions.Contracts;
using Togo.Application.Prescriptions.UseCases;
using Togo.Application.Tutors;

namespace Togo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/attendances/{attendanceId:long}/prescriptions")]
public class PrescriptionsController : ControllerBase
{
    private readonly CreatePrescriptionUseCase _createPrescriptionUseCase;
    private readonly ListPrescriptionsByAttendanceUseCase _listPrescriptionsByAttendanceUseCase;
    private readonly ILogger<PrescriptionsController> _logger;

    public PrescriptionsController(
        CreatePrescriptionUseCase createPrescriptionUseCase,
        ListPrescriptionsByAttendanceUseCase listPrescriptionsByAttendanceUseCase,
        ILogger<PrescriptionsController> logger)
    {
        _createPrescriptionUseCase = createPrescriptionUseCase;
        _listPrescriptionsByAttendanceUseCase = listPrescriptionsByAttendanceUseCase;
        _logger = logger;
    }

    [Authorize(Policy = PrescriptionPolicies.Read)]
    [HttpGet]
    public async Task<IActionResult> ListByAttendance(long attendanceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Prescription list request received. AttendanceId: {AttendanceId}", attendanceId);

        var result = await _listPrescriptionsByAttendanceUseCase.ExecuteAsync(attendanceId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize(Policy = PrescriptionPolicies.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        long attendanceId,
        [FromBody] CreatePrescriptionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Prescription creation request received. AttendanceId: {AttendanceId}", attendanceId);

        var result = await _createPrescriptionUseCase.ExecuteAsync(attendanceId, request, cancellationToken);
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
