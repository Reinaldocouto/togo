using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class UpdateMedicalRecordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(MedicalRecord.Create(patientId, "old", "{\"v\":1}", DateTime.UtcNow.AddHours(-1)));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("  new note ", "  {\"v\":2}  "), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("new note", result.Data.GeneralNotes);
        Assert.Equal("{\"v\":2}", result.Data.FlagsJson);
        Assert.Equal(1, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid()
    {
        var repository = new FakeMedicalRecordRepository();
        var result = await CreateUseCase(repository, new FakePetRepository())
            .ExecuteAsync(0, new UpdateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Patient id is invalid.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var repository = new FakeMedicalRecordRepository();
        var result = await CreateUseCase(repository, new FakePetRepository())
            .ExecuteAsync(99, new UpdateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Patient not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenMedicalRecordDoesNotExist()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDefensiveNotFound_WhenRepositoryReturnsNullAfterValidations()
    {
        var repository = new FakeMedicalRecordRepository { ReturnNullOnGetByPatientId = true };
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(MedicalRecord.Create(patientId, "note", "{}", DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    private static UpdateMedicalRecordUseCase CreateUseCase(FakeMedicalRecordRepository repository, FakePetRepository petRepository)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var existsValidator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());

        return new UpdateMedicalRecordUseCase(repository, patientValidator, existsValidator, new TestLogger<UpdateMedicalRecordUseCase>());
    }
}
