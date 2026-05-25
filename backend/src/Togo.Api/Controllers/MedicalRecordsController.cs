using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.Tutors;

namespace Togo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/patients/{patientId:long}/medical-record")]
public class MedicalRecordsController : ControllerBase
{
    private const string GetMedicalRecordByPatientIdRouteName = "GetMedicalRecordByPatientId";
    private readonly CreateMedicalRecordUseCase _createMedicalRecordUseCase;
    private readonly GetMedicalRecordByPatientIdUseCase _getMedicalRecordByPatientIdUseCase;
    private readonly UpdateMedicalRecordUseCase _updateMedicalRecordUseCase;
    private readonly ILogger<MedicalRecordsController> _logger;

    public MedicalRecordsController(
        CreateMedicalRecordUseCase createMedicalRecordUseCase,
        GetMedicalRecordByPatientIdUseCase getMedicalRecordByPatientIdUseCase,
        UpdateMedicalRecordUseCase updateMedicalRecordUseCase,
        ILogger<MedicalRecordsController> logger)
    {
        _createMedicalRecordUseCase = createMedicalRecordUseCase;
        _getMedicalRecordByPatientIdUseCase = getMedicalRecordByPatientIdUseCase;
        _updateMedicalRecordUseCase = updateMedicalRecordUseCase;
        _logger = logger;
    }

    [HttpGet(Name = GetMedicalRecordByPatientIdRouteName)]
    [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Medical record query request received. PatientId: {PatientId}", patientId);

        var result = await _getMedicalRecordByPatientIdUseCase.ExecuteAsync(patientId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync(
        long patientId,
        [FromBody] CreateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Medical record creation request received. PatientId: {PatientId}", patientId);

        var result = await _createMedicalRecordUseCase.ExecuteAsync(patientId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtRoute(
            GetMedicalRecordByPatientIdRouteName,
            new { patientId = result.Data!.PatientId },
            result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        long patientId,
        [FromBody] UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Medical record update request received. PatientId: {PatientId}", patientId);

        var result = await _updateMedicalRecordUseCase.ExecuteAsync(patientId, request, cancellationToken);
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
