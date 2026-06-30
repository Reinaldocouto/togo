using System.Text.Json;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Auditing;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Security;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class CancelAttendanceUseCaseTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenIdIsInvalid(long id)
    {
        var repository = new FakeAttendanceRepository();
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(repository, auditLogWriter: auditLogWriter);

        var result = await useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance id is invalid.", result.Error);
        Assert.Null(result.Data);
        Assert.Equal(0, repository.GetByIdCallsCount);
        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist()
    {
        var repository = new FakeAttendanceRepository();
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(repository, auditLogWriter: auditLogWriter);

        var result = await useCase.ExecuteAsync(999, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance not found.", result.Error);
        Assert.Null(result.Data);
        Assert.Equal(1, repository.GetByIdCallsCount);
        Assert.Equal(0, repository.UpdateCallsCount);
        Assert.Empty(auditLogWriter.Events);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAttendanceIsOpen()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 223;
        var attendance = Attendance.Create(1, 12, "ATT-CANCEL-001", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { CurrentUser = new CurrentUserInfo(CurrentUserId, "Receptionist", true) };
        var useCase = CreateUseCase(repository, currentUserService, auditLogWriter);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(AttendanceStatus.Canceled, result.Data.Status);
        Assert.Null(result.Data.ClosedAt);
        Assert.Equal(1, repository.UpdateCallsCount);

        var auditEvent = Assert.Single(auditLogWriter.Events);
        Assert.Equal(nameof(Attendance), auditEvent.EntityName);
        Assert.Equal(attendance.Id.ToString(), auditEvent.EntityId);
        Assert.Equal(AttendanceAuditActions.Canceled, auditEvent.Action);
        Assert.Equal(CurrentUserId, auditEvent.UserId);
        Assert.Equal("Receptionist", auditEvent.UserProfile);
        AssertAuditMetadata(auditEvent.MetadataJson, attendance.PatientId, AttendanceStatus.Canceled);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsClosed()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 224;
        var attendance = Attendance.Create(1, 12, "ATT-CANCEL-002", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        attendance.Close(new DateTime(2026, 03, 10, 10, 0, 0, DateTimeKind.Utc), TestUserId, TestCreatedAt.AddHours(1));
        repository.AddAttendanceForLookup(lookupId, attendance);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { CurrentUser = new CurrentUserInfo(CurrentUserId, "Receptionist", true) };
        var useCase = CreateUseCase(repository, currentUserService, auditLogWriter);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Closed attendance cannot be canceled", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsAlreadyCanceled()
    {
        var repository = new FakeAttendanceRepository();
        const long lookupId = 225;
        var attendance = Attendance.Create(1, 12, "ATT-CANCEL-003", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1));
        repository.AddAttendanceForLookup(lookupId, attendance);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { CurrentUser = new CurrentUserInfo(CurrentUserId, "Receptionist", true) };
        var useCase = CreateUseCase(repository, currentUserService, auditLogWriter);

        var result = await useCase.ExecuteAsync(lookupId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Attendance is already canceled", result.Error);
        Assert.Equal(0, repository.UpdateCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowCurrentUserResolutionException_WhenCurrentUserDoesNotResolve()
    {
        var repository = new FakeAttendanceRepository();
        var lookupId = 123L;
        var attendance = Attendance.Create(1, 12, "ATT-USER-FAIL", new DateTime(2026, 03, 10, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        repository.AddAttendanceForLookup(lookupId, attendance);
        var currentUserService = new FakeCurrentUserService(CurrentUserId) { ThrowResolutionException = true };
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(repository, currentUserService, auditLogWriter);

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() =>
            useCase.ExecuteAsync(lookupId, CancellationToken.None));

        Assert.Empty(auditLogWriter.Events);
    }

    private static CancelAttendanceUseCase CreateUseCase(
        FakeAttendanceRepository repository,
        FakeCurrentUserService? currentUserService = null,
        FakeClinicalAuditLogWriter? auditLogWriter = null) =>
        new(
            repository,
            currentUserService ?? new FakeCurrentUserService(CurrentUserId),
            auditLogWriter ?? new FakeClinicalAuditLogWriter(),
            new TestLogger<CancelAttendanceUseCase>());

    private static void AssertAuditMetadata(string? metadataJson, long expectedPatientId, AttendanceStatus expectedStatus)
    {
        Assert.False(string.IsNullOrWhiteSpace(metadataJson));
        using var metadata = JsonDocument.Parse(metadataJson);
        var root = metadata.RootElement;

        Assert.Equal(2, root.EnumerateObject().Count());
        Assert.Equal(expectedPatientId, root.GetProperty("PatientId").GetInt64());
        Assert.Equal(expectedStatus.ToString(), root.GetProperty("Status").GetString());
        Assert.False(root.TryGetProperty("GeneralNotes", out _));
        Assert.False(root.TryGetProperty("FlagsJson", out _));
        Assert.False(root.TryGetProperty("Prescription", out _));
        Assert.False(root.TryGetProperty("ClinicalEvolution", out _));
    }
}
