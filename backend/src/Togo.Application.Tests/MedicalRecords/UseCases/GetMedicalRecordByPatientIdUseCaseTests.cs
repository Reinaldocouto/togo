using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class GetMedicalRecordByPatientIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenMedicalRecordExists()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(MedicalRecord.Create(1, patientId, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));
        var useCase = CreateUseCase(repository, petRepository);

        var result = await useCase.ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal("note", result.Data.GeneralNotes);
        Assert.Equal("{}", result.Data.FlagsJson);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid()
    {
        var result = await CreateUseCase(new FakeMedicalRecordRepository(), new FakePetRepository())
            .ExecuteAsync(0, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var result = await CreateUseCase(new FakeMedicalRecordRepository(), new FakePetRepository())
            .ExecuteAsync(11, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Patient not found.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenMedicalRecordDoesNotExist()
    {
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();

        var result = await CreateUseCase(new FakeMedicalRecordRepository(), petRepository)
            .ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenMedicalRecordIsSoftDeleted()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var medicalRecord = MedicalRecord.Create(1, patientId, "deleted note", "{\"deleted\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-2));
        medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow.AddHours(-1));
        repository.AddExisting(medicalRecord);

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDefensiveNotFound_WhenRepositoryReturnsNullAfterValidations()
    {
        var repository = new FakeMedicalRecordRepository { ReturnNullOnGetByPatientId = true };
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(MedicalRecord.Create(1, patientId, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationToken_ToRepositoryOnGet()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        repository.AddExisting(MedicalRecord.Create(1, patientId, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));
        using var cts = new CancellationTokenSource();

        await CreateUseCase(repository, petRepository).ExecuteAsync(patientId, cts.Token);

        Assert.Equal(cts.Token, repository.LastExistsByPatientIdCancellationToken);
        Assert.Equal(cts.Token, repository.LastGetByPatientIdCancellationToken);
    }

    private static GetMedicalRecordByPatientIdUseCase CreateUseCase(FakeMedicalRecordRepository repository, FakePetRepository petRepository)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var existsValidator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());

        return new GetMedicalRecordByPatientIdUseCase(repository, patientValidator, existsValidator, new TestLogger<GetMedicalRecordByPatientIdUseCase>());
    }
}
