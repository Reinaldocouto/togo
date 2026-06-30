using Togo.Application.Auditing;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Security;
using Togo.Application.Tutors;

namespace Togo.Application.Tests.MedicalRecords.UseCases;

public sealed class CreateMedicalRecordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationToken_ToRepositoryOnCreate()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        using var cts = new CancellationTokenSource();

        await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("note", "{}"), cts.Token);

        Assert.Equal(cts.Token, repository.LastExistsIncludingSoftDeletedByPatientIdCancellationToken);
        Assert.Equal(cts.Token, repository.LastAddCancellationToken);
    }

    private static readonly Guid CurrentUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet(clinicId: 42);
        var request = new CreateMedicalRecordRequest("  General  ", "  {\"flag\":true}  ");
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(CurrentUserId)
        {
            CurrentUser = new CurrentUserInfo(CurrentUserId, Profile: "Veterinarian", IsAuthenticated: true)
        };
        var useCase = CreateUseCase(repository, petRepository, currentUserService: currentUserService, auditLogWriter: auditLogWriter);

        var result = await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.ClinicId);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal("General", result.Data.GeneralNotes);
        Assert.Equal("{\"flag\":true}", result.Data.FlagsJson);
        Assert.NotEqual(default, result.Data.UpdatedAt);
        Assert.Equal(1, repository.AddCallsCount);
        var persisted = Assert.Single(repository.Items);
        Assert.Equal(42, persisted.ClinicId);
        Assert.Equal(CurrentUserId, persisted.CreatedByUserId);
        Assert.Equal(CurrentUserId, persisted.UpdatedByUserId);
        Assert.NotEqual(default, persisted.CreatedAt);
        Assert.Equal(persisted.CreatedAt, persisted.UpdatedAt);

        var auditEvent = Assert.Single(auditLogWriter.Events);
        Assert.Equal(nameof(Togo.Domain.Entities.MedicalRecord), auditEvent.EntityName);
        Assert.Equal(persisted.Id.ToString(), auditEvent.EntityId);
        Assert.Equal(MedicalRecordAuditActions.Created, auditEvent.Action);
        Assert.Equal(CurrentUserId, auditEvent.UserId);
        Assert.Equal("Veterinarian", auditEvent.UserProfile);
        Assert.Equal(DateTimeKind.Utc, auditEvent.OccurredAt.Kind);
        Assert.NotNull(auditEvent.MetadataJson);
        Assert.Contains("\"ClinicId\":42", auditEvent.MetadataJson);
        Assert.Contains($"\"PatientId\":{patientId}", auditEvent.MetadataJson);
        Assert.DoesNotContain(request.GeneralNotes!, auditEvent.MetadataJson);
        Assert.DoesNotContain(request.FlagsJson!, auditEvent.MetadataJson);
        Assert.DoesNotContain(nameof(CreateMedicalRecordRequest.GeneralNotes), auditEvent.MetadataJson);
        Assert.DoesNotContain(nameof(CreateMedicalRecordRequest.FlagsJson), auditEvent.MetadataJson);
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
        repository.AddExisting(Togo.Domain.Entities.MedicalRecord.Create(1, patientId, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var result = await CreateUseCase(repository, petRepository)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("new", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("Patient already has a medical record.", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenOnlyExistingMedicalRecordIsSoftDeleted()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var deletedRecord = Togo.Domain.Entities.MedicalRecord.Create(1, patientId, "deleted", "{\"old\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-3));
        deletedRecord.SoftDelete(CurrentUserId, DateTime.UtcNow.AddHours(-2));
        repository.AddExisting(deletedRecord);
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("new active", "{\"new\":true}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("Patient already has a medical record.", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
        Assert.Single(repository.Items);
        Assert.True(deletedRecord.IsDeleted);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Equal(patientId, repository.LastExistsIncludingSoftDeletedByPatientIdInput);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenUniqueConstraintIsViolatedDuringAdd()
    {
        var repository = new FakeMedicalRecordRepository { ThrowMedicalRecordAlreadyExistsOnAdd = true };
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("new", "{}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Patient already has a medical record.", result.Error);
        Assert.Equal(1, repository.AddCallsCount);
        Assert.Empty(repository.Items);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
        Assert.Equal(patientId, repository.LastExistsIncludingSoftDeletedByPatientIdInput);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateUnexpectedPersistenceException_WhenAddFailsForUnrelatedReason()
    {
        var expectedException = new InvalidOperationException("Simulated unrelated persistence failure.");
        var repository = new FakeMedicalRecordRepository { ExceptionToThrowOnAdd = expectedException };
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
                .ExecuteAsync(patientId, new CreateMedicalRecordRequest("new", "{}"), CancellationToken.None));

        Assert.Same(expectedException, actualException);
        Assert.Equal(1, repository.AddCallsCount);
        Assert.Empty(repository.Items);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenFlagsJsonIsInvalid(string flagsJson)
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, petRepository, auditLogWriter: auditLogWriter)
            .ExecuteAsync(patientId, new CreateMedicalRecordRequest("SENSITIVE_NOTE", flagsJson), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.StartsWith("FlagsJson must be a valid JSON object.", result.Error);
        Assert.DoesNotContain(flagsJson, result.Error);
        Assert.DoesNotContain("SENSITIVE_NOTE", result.Error);
        Assert.Equal(0, repository.AddCallsCount);
        Assert.Empty(repository.Items);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotLogSensitiveFields()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var logger = new TestLogger<CreateMedicalRecordUseCase>();
        var useCase = CreateUseCase(repository, petRepository, logger);
        var request = new CreateMedicalRecordRequest("SENSITIVE_NOTE", "{\"secret\":\"SENSITIVE_JSON\"}");

        await useCase.ExecuteAsync(patientId, request, CancellationToken.None);

        var combined = string.Join("|", logger.Entries.Select(x => x.Message));
        Assert.DoesNotContain("SENSITIVE_NOTE", combined);
        Assert.DoesNotContain("SENSITIVE_JSON", combined);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotWriteAuditLog_WhenCreationFailsValidation()
    {
        var repository = new FakeMedicalRecordRepository();
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        var result = await CreateUseCase(repository, new FakePetRepository(), auditLogWriter: auditLogWriter)
            .ExecuteAsync(0, new CreateMedicalRecordRequest("SENSITIVE_NOTE", "{\"secret\":\"SENSITIVE_FLAGS_JSON\"}"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailSafely_WhenCurrentUserCannotBeResolved()
    {
        var repository = new FakeMedicalRecordRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { ThrowResolutionException = true };
        var useCase = CreateUseCase(repository, petRepository, currentUserService: currentUserService);

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() =>
            useCase.ExecuteAsync(patientId, new CreateMedicalRecordRequest("note", "{}"), CancellationToken.None));
        Assert.Equal(0, repository.AddCallsCount);
    }

    private static CreateMedicalRecordUseCase CreateUseCase(
        FakeMedicalRecordRepository repository,
        FakePetRepository petRepository,
        TestLogger<CreateMedicalRecordUseCase>? logger = null,
        FakeCurrentUserService? currentUserService = null,
        FakeClinicalAuditLogWriter? auditLogWriter = null)
    {
        var patientValidator = new MedicalRecordPatientExistsValidator(petRepository, new TestLogger<MedicalRecordPatientExistsValidator>());
        var uniquenessValidator = new MedicalRecordUniquenessValidator(repository, new TestLogger<MedicalRecordUniquenessValidator>());

        return new CreateMedicalRecordUseCase(
            repository,
            patientValidator,
            uniquenessValidator,
            currentUserService ?? new FakeCurrentUserService(CurrentUserId),
            auditLogWriter ?? new FakeClinicalAuditLogWriter(),
            logger ?? new TestLogger<CreateMedicalRecordUseCase>());
    }
}
