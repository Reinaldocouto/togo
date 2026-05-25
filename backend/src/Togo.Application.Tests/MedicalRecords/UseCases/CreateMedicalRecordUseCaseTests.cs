using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class CreateMedicalRecordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var request = new CreateMedicalRecordRequest("  General  ", "  {\"flag\":true}  ");
        var useCase = CreateUseCase(repository, petRepository);

        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal("General", result.Data.GeneralNotes);
        Assert.Equal("{\"flag\":true}", result.Data.FlagsJson);
        Assert.NotEqual(default, result.Data.UpdatedAt);
        Assert.Equal(1, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid()
    {
        var repository = new FakeMedicalRecordRepository();
        var result = await CreateUseCase(repository, new FakePetRepository())
            .ExecuteAsync(0, new CreateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Patient id is invalid.", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var repository = new FakeMedicalRecordRepository();
        var result = await CreateUseCase(repository, new FakePetRepository())
            .ExecuteAsync(99, new CreateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Patient not found.", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenMedicalRecordAlreadyExists()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(Togo.Domain.Entities.MedicalRecord.Create(patientId, "note", "{}", DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("new", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("Patient already has a medical record.", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
    }

        [Fact]
    public async Task ExecuteAsync_ShouldNotLogSensitiveFields()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var logger = new TestLogger<CreateMedicalRecordUseCase>();
        var useCase = CreateUseCase(repository, petRepository, logger);
        var request = new CreateMedicalRecordRequest("SENSITIVE_NOTE", "SENSITIVE_JSON");

        await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        var combined = string.Join("|", logger.Entries.Select(x => x.Message));
        Assert.DoesNotContain("SENSITIVE_NOTE", combined);
        Assert.DoesNotContain("SENSITIVE_JSON", combined);
    }

    private static CreateMedicalRecordUseCase CreateUseCase(
        FakeMedicalRecordRepository repository,
        FakePetRepository petRepository,
        TestLogger<CreateMedicalRecordUseCase>? logger = null)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var uniquenessValidator = new MedicalRecordUniquenessValidator(repository, new TestLogger<MedicalRecordUniquenessValidator>());

        return new CreateMedicalRecordUseCase(repository, patientValidator, uniquenessValidator, logger ?? new TestLogger<CreateMedicalRecordUseCase>());
    }
}
