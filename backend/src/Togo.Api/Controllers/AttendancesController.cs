using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Tutors;

namespace Togo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/attendances")]
public class AttendancesController : ControllerBase
{
    private readonly CreateAttendanceUseCase _createAttendanceUseCase;
    private readonly GetAttendanceByIdUseCase _getAttendanceByIdUseCase;
    private readonly ListAttendancesUseCase _listAttendancesUseCase;
    private readonly CloseAttendanceUseCase _closeAttendanceUseCase;
    private readonly CancelAttendanceUseCase _cancelAttendanceUseCase;
    private readonly ILogger<AttendancesController> _logger;

    public AttendancesController(
        CreateAttendanceUseCase createAttendanceUseCase,
        GetAttendanceByIdUseCase getAttendanceByIdUseCase,
        ListAttendancesUseCase listAttendancesUseCase,
        CloseAttendanceUseCase closeAttendanceUseCase,
        CancelAttendanceUseCase cancelAttendanceUseCase,
        ILogger<AttendancesController> logger)
    {
        _createAttendanceUseCase = createAttendanceUseCase;
        _getAttendanceByIdUseCase = getAttendanceByIdUseCase;
        _listAttendancesUseCase = listAttendancesUseCase;
        _closeAttendanceUseCase = closeAttendanceUseCase;
        _cancelAttendanceUseCase = cancelAttendanceUseCase;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attendance list request received");

        var result = await _listAttendancesUseCase.ExecuteAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attendance detail request received. AttendanceId: {AttendanceId}", id);

        var result = await _getAttendanceByIdUseCase.ExecuteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attendance creation request received. PatientId: {PatientId}", request.PatientId);

        var result = await _createAttendanceUseCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPatch("{id:long}/close")]
    public async Task<IActionResult> Close(long id, [FromBody] CloseAttendanceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attendance close request received. AttendanceId: {AttendanceId}", id);

        var result = await _closeAttendanceUseCase.ExecuteAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPatch("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attendance cancel request received. AttendanceId: {AttendanceId}", id);

        var result = await _cancelAttendanceUseCase.ExecuteAsync(id, cancellationToken);
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
