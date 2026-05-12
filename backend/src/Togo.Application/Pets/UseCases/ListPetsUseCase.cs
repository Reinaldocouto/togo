using Microsoft.Extensions.Logging;
using Togo.Application.Pets.Contracts;
using Togo.Application.Tutors;

namespace Togo.Application.Pets.UseCases;

public class ListPetsUseCase
{
    private readonly IPetRepository _petRepository;
    private readonly ILogger<ListPetsUseCase> _logger;

    public ListPetsUseCase(
        IPetRepository petRepository,
        ILogger<ListPetsUseCase> logger)
    {
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<IReadOnlyList<PetListItemResponse>>> ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing pets");

        var pets = await _petRepository.ListAsync(cancellationToken);
        var responses = PetMappings.ToListItemResponses(pets);

        _logger.LogInformation("Pets listed successfully. Count: {Count}", responses.Count);

        return ApplicationResult<IReadOnlyList<PetListItemResponse>>.Success(responses);
    }
}
