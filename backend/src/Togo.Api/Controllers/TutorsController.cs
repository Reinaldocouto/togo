using Microsoft.AspNetCore.Mvc;
using Togo.Application.Tutors;
using Togo.Application.Tutors.Contracts;
using Togo.Application.Tutors.UseCases;

namespace Togo.Api.Controllers;

[ApiController]
[Route("api/tutors")]
public class TutorsController : ControllerBase
{
    private readonly ListTutorsUseCase _listTutorsUseCase;
    private readonly GetTutorByIdUseCase _getTutorByIdUseCase;
    private readonly CreateTutorUseCase _createTutorUseCase;
    private readonly UpdateTutorUseCase _updateTutorUseCase;
    private readonly DeleteTutorUseCase _deleteTutorUseCase;

    public TutorsController(
        ListTutorsUseCase listTutorsUseCase,
        GetTutorByIdUseCase getTutorByIdUseCase,
        CreateTutorUseCase createTutorUseCase,
        UpdateTutorUseCase updateTutorUseCase,
        DeleteTutorUseCase deleteTutorUseCase)
    {
        _listTutorsUseCase = listTutorsUseCase;
        _getTutorByIdUseCase = getTutorByIdUseCase;
        _createTutorUseCase = createTutorUseCase;
        _updateTutorUseCase = updateTutorUseCase;
        _deleteTutorUseCase = deleteTutorUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _listTutorsUseCase.ExecuteAsync(cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await _getTutorByIdUseCase.ExecuteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTutorRequest request, CancellationToken cancellationToken)
    {
        var result = await _createTutorUseCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTutorRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateTutorUseCase.ExecuteAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var result = await _deleteTutorUseCase.ExecuteAsync(id, cancellationToken);
        return result.Type switch
        {
            ApplicationResultType.Success => NoContent(),
            ApplicationResultType.NotFound => NotFound(new { message = result.Error }),
            ApplicationResultType.Conflict => Conflict(new { message = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Error ?? "Unexpected error." })
        };
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
