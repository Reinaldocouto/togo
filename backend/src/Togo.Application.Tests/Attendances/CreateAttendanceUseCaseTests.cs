using System.Text.Json;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Auditing;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Attendances.Validators;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class CreateAttendanceUseCaseTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet(clinicId: 42);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { CurrentUser = new CurrentUserInfo(CurrentUserId, "Veterinarian", true) };
        var useCase = CreateUseCase(attendanceRepository, petRepository, currentUserService, auditLogWriter);
        var request = CreateValidRequest(patientId: patientId);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.ClinicId);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(request.AttendanceNumber, result.Data.AttendanceNumber);
        Assert.Equal(request.OpenedAt, result.Data.OpenedAt);
        Assert.Equal(AttendanceStatus.Open, result.Data.Status);
        Assert.Null(result.Data.ClosedAt);
        Assert.Equal(request.Type, result.Data.Type);

        Assert.Equal(1, attendanceRepository.AddCallsCount);

        var auditEvent = Assert.Single(auditLogWriter.Events);
        Assert.Equal(nameof(Togo.Domain.Entities.Attendance), auditEvent.EntityName);
        Assert.Equal(AttendanceAuditActions.Created, auditEvent.Action);
        Assert.Equal(CurrentUserId, auditEvent.UserId);
        Assert.Equal("Veterinarian", auditEvent.UserProfile);
        Assert.Equal(result.Data.Id.ToString(), auditEvent.EntityId);
        AssertAuditMetadata(auditEvent.MetadataJson, 42, patientId, AttendanceStatus.Open);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenPatientDoesNotExist()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(attendanceRepository, petRepository, auditLogWriter: auditLogWriter);
        var request = CreateValidRequest(patientId: 999);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Patient not found.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
        Assert.Null(attendanceRepository.LastExistsByAttendanceNumberInput);
        Assert.Null(attendanceRepository.LastHasOpenAttendancePatientIdInput);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceNumberAlreadyExists()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddExistingAttendanceNumber("ATT-001");
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(attendanceRepository, petRepository, auditLogWriter: auditLogWriter);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "ATT-001");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("An attendance with this number already exists.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
        Assert.Null(attendanceRepository.LastHasOpenAttendancePatientIdInput);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenPatientAlreadyHasOpenAttendance()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        attendanceRepository.AddOpenAttendancePatient(patientId);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(attendanceRepository, petRepository, auditLogWriter: auditLogWriter);
        var request = CreateValidRequest(patientId: patientId);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("Patient already has an open attendance.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenAttendanceNumberIsInvalid()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "   ");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Attendance number is required.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenDomainThrowsArgumentException()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, openedAt: default(DateTime));

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.StartsWith("Date is required", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimAttendanceNumber_WhenCreatingAttendance()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "  ATT-TRIM-001  ");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.Equal("ATT-TRIM-001", result.Data!.AttendanceNumber);

        var persisted = (await attendanceRepository.ListByPatientIdAsync(patientId, CancellationToken.None)).Single();
        Assert.Equal("ATT-TRIM-001", persisted.AttendanceNumber);
        Assert.Equal(1, persisted.ClinicId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowCurrentUserResolutionException_WhenCurrentUserDoesNotResolve()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { ThrowResolutionException = true };
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(attendanceRepository, petRepository, currentUserService, auditLogWriter);

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() =>
            useCase.ExecuteAsync(CreateValidRequest(patientId: patientId), CancellationToken.None));

        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public void CreateAttendanceRequest_ShouldNotExposeClinicId()
    {
        var property = typeof(CreateAttendanceRequest).GetProperty("ClinicId");

        Assert.Null(property);
    }

    private static CreateAttendanceUseCase CreateUseCase(
        FakeAttendanceRepository attendanceRepository,
        FakePetRepository petRepository,
        FakeCurrentUserService? currentUserService = null,
        FakeClinicalAuditLogWriter? auditLogWriter = null)
    {
        var patientExistsValidator = new AttendancePatientExistsValidator(
            petRepository,
            new TestLogger<AttendancePatientExistsValidator>());
        var attendanceNumberUniqueValidator = new AttendanceNumberUniqueValidator(
            attendanceRepository,
            new TestLogger<AttendanceNumberUniqueValidator>());
        var openAttendanceValidator = new OpenAttendanceValidator(
            attendanceRepository,
            new TestLogger<OpenAttendanceValidator>());

        return new CreateAttendanceUseCase(
            attendanceRepository,
            patientExistsValidator,
            attendanceNumberUniqueValidator,
            openAttendanceValidator,
            currentUserService ?? new FakeCurrentUserService(CurrentUserId),
            auditLogWriter ?? new FakeClinicalAuditLogWriter(),
            new TestLogger<CreateAttendanceUseCase>());
    }

    private static void AssertAuditMetadata(string? metadataJson, long expectedClinicId, long expectedPatientId, AttendanceStatus expectedStatus)
    {
        Assert.False(string.IsNullOrWhiteSpace(metadataJson));
        using var metadata = JsonDocument.Parse(metadataJson);
        var root = metadata.RootElement;

        Assert.Equal(3, root.EnumerateObject().Count());
        Assert.Equal(expectedClinicId, root.GetProperty("ClinicId").GetInt64());
        Assert.Equal(expectedPatientId, root.GetProperty("PatientId").GetInt64());
        Assert.Equal(expectedStatus.ToString(), root.GetProperty("Status").GetString());
        Assert.False(root.TryGetProperty("GeneralNotes", out _));
        Assert.False(root.TryGetProperty("FlagsJson", out _));
        Assert.False(root.TryGetProperty("Prescription", out _));
        Assert.False(root.TryGetProperty("ClinicalEvolution", out _));
    }

    private static CreateAttendanceRequest CreateValidRequest(
        long patientId,
        string attendanceNumber = "ATT-001",
        DateTime? openedAt = null,
        AttendanceType type = AttendanceType.Consultation)
        => new(
            patientId,
            attendanceNumber,
            openedAt ?? new DateTime(2026, 01, 15, 10, 30, 00, DateTimeKind.Utc),
            type);
}
