using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class SoftDeleteMedicalRecordUseCaseTests
{
    private static readonly Guid CreatorUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid DeletingUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public async Task ExecuteAsync_ShouldResolveCurrentUserAndPersistSoftDeleteFields_WhenRecordExists()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var medicalRecord = MedicalRecord.Create(patientId, "clinical note", "{\"flag\":true}", CreatorUserId, createdAt);
        repository.AddExisting(medicalRecord);
        var currentUserService = new FakeCurrentUserService(DeletingUserId)
        {
            CurrentUser = new CurrentUserInfo(DeletingUserId, Profile: "Veterinarian", IsAuthenticated: true)
        };

        var result = await CreateUseCase(repository, petRepository, currentUserService).ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.UpdateCallsCount);
        Assert.Equal(0, repository.AddCallsCount);
        var persisted = Assert.Single(repository.Items);
        Assert.True(persisted.IsDeleted);
        Assert.Equal(DeletingUserId, persisted.DeletedByUserId);
        Assert.NotNull(persisted.DeletedAt);
        Assert.Equal(DateTimeKind.Utc, persisted.DeletedAt!.Value.Kind);
        Assert.True(persisted.DeletedAt.Value >= createdAt);
        Assert.Equal(CreatorUserId, persisted.CreatedByUserId);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.Equal(CreatorUserId, persisted.UpdatedByUserId);
        Assert.Equal(createdAt, persisted.UpdatedAt);
        Assert.Equal("clinical note", persisted.GeneralNotes);
        Assert.Equal("{\"flag\":true}", persisted.FlagsJson);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid()
    {
        var repository = new FakeMedicalRecordRepository();

        var result = await CreateUseCase(repository, new FakePetRepository()).ExecuteAsync(0, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Patient id is invalid.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var repository = new FakeMedicalRecordRepository();

        var result = await CreateUseCase(repository, new FakePetRepository()).ExecuteAsync(99, CancellationToken.None);

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

        var result = await CreateUseCase(repository, petRepository).ExecuteAsync(patientId, CancellationToken.None);

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
        repository.AddExisting(MedicalRecord.Create(patientId, "note", "{}", CreatorUserId, DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository).ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailSafely_WhenCurrentUserCannotBeResolved()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var medicalRecord = MedicalRecord.Create(patientId, "note", "{}", CreatorUserId, DateTime.UtcNow.AddHours(-1));
        repository.AddExisting(medicalRecord);
        var currentUserService = new FakeCurrentUserService(DeletingUserId) { ThrowResolutionException = true };

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() =>
            CreateUseCase(repository, petRepository, currentUserService).ExecuteAsync(patientId, CancellationToken.None));

        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.False(medicalRecord.IsDeleted);
        Assert.Null(medicalRecord.DeletedAt);
        Assert.Null(medicalRecord.DeletedByUserId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenRecordIsAlreadySoftDeleted()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var medicalRecord = MedicalRecord.Create(patientId, "note", "{}", CreatorUserId, DateTime.UtcNow.AddHours(-2));
        medicalRecord.SoftDelete(DeletingUserId, DateTime.UtcNow.AddHours(-1));
        repository.AddExisting(medicalRecord);

        var result = await CreateUseCase(repository, petRepository).ExecuteAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    private static SoftDeleteMedicalRecordUseCase CreateUseCase(
        FakeMedicalRecordRepository repository,
        FakePetRepository petRepository,
        FakeCurrentUserService? currentUserService = null)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var existsValidator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());

        return new SoftDeleteMedicalRecordUseCase(
            repository,
            patientValidator,
            existsValidator,
            currentUserService ?? new FakeCurrentUserService(DeletingUserId),
            new TestLogger<SoftDeleteMedicalRecordUseCase>());
    }
}
