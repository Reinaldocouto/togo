using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Application.Pets.Contracts;
using Togo.Application.Pets.UseCases;
using Togo.Application.Tutors;

namespace Togo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly ListPetsUseCase _listPetsUseCase;
    private readonly GetPetByIdUseCase _getPetByIdUseCase;
    private readonly CreatePetUseCase _createPetUseCase;
    private readonly UpdatePetUseCase _updatePetUseCase;
    private readonly DeletePetUseCase _deletePetUseCase;
    private readonly ILogger<PetsController> _logger;

    public PetsController(
        ListPetsUseCase listPetsUseCase,
        GetPetByIdUseCase getPetByIdUseCase,
        CreatePetUseCase createPetUseCase,
        UpdatePetUseCase updatePetUseCase,
        DeletePetUseCase deletePetUseCase,
        ILogger<PetsController> logger)
    {
        _listPetsUseCase = listPetsUseCase;
        _getPetByIdUseCase = getPetByIdUseCase;
        _createPetUseCase = createPetUseCase;
        _updatePetUseCase = updatePetUseCase;
        _deletePetUseCase = deletePetUseCase;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pet list request received");

        var result = await _listPetsUseCase.ExecuteAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{patientId:long}")]
    public async Task<IActionResult> GetById(long patientId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pet detail request received. PatientId: {PatientId}", patientId);

        var result = await _getPetByIdUseCase.ExecuteAsync(patientId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePetRequest request, CancellationToken cancellationToken)
    {
        var hasMicrochip = !string.IsNullOrWhiteSpace(request.Microchip);
        _logger.LogInformation(
            "Pet creation request received. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
            request.TutorId,
            hasMicrochip);

        var result = await _createPetUseCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { patientId = result.Data!.PatientId },
            result.Data);
    }

    [HttpPut("{patientId:long}")]
    public async Task<IActionResult> Update(
        long patientId,
        [FromBody] UpdatePetRequest request,
        CancellationToken cancellationToken)
    {
        var hasMicrochip = !string.IsNullOrWhiteSpace(request.Microchip);
        _logger.LogInformation(
            "Pet update request received. PatientId: {PatientId}. TutorId: {TutorId}. HasMicrochip: {HasMicrochip}",
            patientId,
            request.TutorId,
            hasMicrochip);

        var result = await _updatePetUseCase.ExecuteAsync(patientId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{patientId:long}")]
    public async Task<IActionResult> Delete(long patientId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pet delete request received. PatientId: {PatientId}", patientId);

        var result = await _deletePetUseCase.ExecuteAsync(patientId, cancellationToken);
        return ToDeleteActionResult(result);
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

    private IActionResult ToDeleteActionResult(ApplicationResult<bool> result)
    {
        return result.Type switch
        {
            ApplicationResultType.Success => NoContent(),
            ApplicationResultType.NotFound => NotFound(new { message = result.Error }),
            ApplicationResultType.ValidationError => BadRequest(new { message = result.Error }),
            ApplicationResultType.Conflict => Conflict(new { message = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Error ?? "Unexpected error." })
        };
    }
}
