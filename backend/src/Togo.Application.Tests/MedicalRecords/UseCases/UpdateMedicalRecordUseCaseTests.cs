using Togo.Application.Auditing;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class UpdateMedicalRecordUseCaseTests
{
    private static readonly Guid CreatorUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid UpdatingUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var createdAt = DateTime.UtcNow.AddHours(-1);
        repository.AddExisting(MedicalRecord.Create(patientId, "old", "{\"v\":1}", CreatorUserId, createdAt));

        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(UpdatingUserId)
        {
            CurrentUser = new CurrentUserInfo(UpdatingUserId, Profile: "Admin", IsAuthenticated: true)
        };

        var result = await CreateUseCase(repository, petRepository, currentUserService, auditLogWriter)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("  new note ", "  {\"v\":2}  "), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("new note", result.Data.GeneralNotes);
        Assert.Equal("{\"v\":2}", result.Data.FlagsJson);
        Assert.Equal(1, repository.UpdateCallsCount);
        var persisted = Assert.Single(repository.Items);
        Assert.Equal(CreatorUserId, persisted.CreatedByUserId);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.Equal(UpdatingUserId, persisted.UpdatedByUserId);
        Assert.True(persisted.UpdatedAt > createdAt);

        var auditEvent = Assert.Single(auditLogWriter.Events);
        Assert.Equal(nameof(MedicalRecord), auditEvent.EntityName);
        Assert.Equal(persisted.Id.ToString(), auditEvent.EntityId);
        Assert.Equal(MedicalRecordAuditActions.Updated, auditEvent.Action);
        Assert.Equal(UpdatingUserId, auditEvent.UserId);
        Assert.Equal("Admin", auditEvent.UserProfile);
        Assert.Equal(DateTimeKind.Utc, auditEvent.OccurredAt.Kind);
        Assert.NotNull(auditEvent.MetadataJson);
        Assert.Contains($"\"PatientId\":{patientId}", auditEvent.MetadataJson);
        Assert.DoesNotContain("new note", auditEvent.MetadataJson);
        Assert.DoesNotContain("{\"v\":2}", auditEvent.MetadataJson);
        Assert.DoesNotContain(nameof(UpdateMedicalRecordRequest.GeneralNotes), auditEvent.MetadataJson);
        Assert.DoesNotContain(nameof(UpdateMedicalRecordRequest.FlagsJson), auditEvent.MetadataJson);
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
        repository.AddExisting(MedicalRecord.Create(patientId, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("note", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }


    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenFlagsJsonIsInvalid(string flagsJson)
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var originalUpdatedAt = DateTime.UtcNow.AddHours(-1);
        var medicalRecord = MedicalRecord.Create(patientId, "old notes", "{\"old\":true}", CreatorUserId, createdAt);
        medicalRecord.UpdateNotes("current notes", "{\"current\":true}", CreatorUserId, originalUpdatedAt);
        repository.AddExisting(medicalRecord);
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("SENSITIVE_NOTE", flagsJson), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.StartsWith("FlagsJson must be a valid JSON object.", result.Error);
        Assert.DoesNotContain(flagsJson, result.Error);
        Assert.DoesNotContain("SENSITIVE_NOTE", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
        Assert.Equal("current notes", medicalRecord.GeneralNotes);
        Assert.Equal("{\"current\":true}", medicalRecord.FlagsJson);
        Assert.Equal(CreatorUserId, medicalRecord.UpdatedByUserId);
        Assert.Equal(originalUpdatedAt, medicalRecord.UpdatedAt);
        Assert.Equal(CreatorUserId, medicalRecord.CreatedByUserId);
        Assert.Equal(createdAt, medicalRecord.CreatedAt);
        Assert.Equal(patientId, medicalRecord.PatientId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFoundAndNotMutateOrAudit_WhenMedicalRecordIsSoftDeleted()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var createdAt = DateTime.UtcNow.AddHours(-3);
        var deletedAt = DateTime.UtcNow.AddHours(-2);
        var medicalRecord = MedicalRecord.Create(patientId, "old deleted note", "{\"v\":1}", CreatorUserId, createdAt);
        medicalRecord.SoftDelete(UpdatingUserId, deletedAt);
        repository.AddExisting(medicalRecord);
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("new note", "{\"v\":2}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Medical record not found.", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Equal("old deleted note", medicalRecord.GeneralNotes);
        Assert.Equal("{\"v\":1}", medicalRecord.FlagsJson);
        Assert.Equal(CreatorUserId, medicalRecord.UpdatedByUserId);
        Assert.Equal(createdAt, medicalRecord.UpdatedAt);
        Assert.True(medicalRecord.IsDeleted);
        Assert.Equal(deletedAt, medicalRecord.DeletedAt);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldNotWriteAuditLog_WhenUpdateFailsValidation()
    {
        var repository = new FakeMedicalRecordRepository();
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, new FakePetRepository(), auditLogWriter: auditLogWriter)
            .ExecuteAsync(0, new UpdateMedicalRecordRequest("SENSITIVE_NOTE", "{\"secret\":\"SENSITIVE_FLAGS_JSON\"}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailSafely_WhenCurrentUserCannotBeResolved()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var medicalRecord = MedicalRecord.Create(patientId, "old", "{}", CreatorUserId, DateTime.UtcNow.AddHours(-1));
        repository.AddExisting(medicalRecord);
        var originalUpdatedAt = medicalRecord.UpdatedAt;
        var currentUserService = new FakeCurrentUserService(UpdatingUserId) { ThrowResolutionException = true };

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() => CreateUseCase(repository, petRepository, currentUserService)
            .ExecuteAsync(patientId, new UpdateMedicalRecordRequest("new", "{}"), CancellationToken.None));

        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Equal(originalUpdatedAt, medicalRecord.UpdatedAt);
        Assert.Equal(CreatorUserId, medicalRecord.UpdatedByUserId);
    }

    private static UpdateMedicalRecordUseCase CreateUseCase(
        FakeMedicalRecordRepository repository,
        FakePetRepository petRepository,
        FakeCurrentUserService? currentUserService = null,
        FakeClinicalAuditLogWriter? auditLogWriter = null)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var existsValidator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());

        return new UpdateMedicalRecordUseCase(
            repository,
            patientValidator,
            existsValidator,
            currentUserService ?? new FakeCurrentUserService(UpdatingUserId),
            auditLogWriter ?? new FakeClinicalAuditLogWriter(),
            new TestLogger<UpdateMedicalRecordUseCase>());
    }
}
