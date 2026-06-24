using Togo.Application.Auditing;
using Togo.Application.Prescriptions.Contracts;
using Togo.Application.Prescriptions.UseCases;
using Togo.Application.Security;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Prescriptions.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Prescriptions;

public class CreatePrescriptionUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime Now = new(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndWriteAuditLogOnce_WhenAttendanceIsOpen()
    {
        var currentUser = new FakeCurrentUserService(UserId) { CurrentUser = new CurrentUserInfo(UserId, "Veterinarian", true) };
        var (uc, repo, audit) = CreateUseCase(AttendanceStatus.Open, currentUser);

        var result = await uc.ExecuteAsync(10, ValidRequest(), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.Equal(1, repo.AddCallsCount);
        Assert.Single(repo.AddedItems);
        Assert.Equal("notes", result.Data!.Notes);
        Assert.Equal(1, audit.WriteCallsCount);
        var auditEvent = Assert.Single(audit.Events);
        Assert.Equal(nameof(Prescription), auditEvent.EntityName);
        Assert.Equal(repo.AddedPrescription!.Id.ToString(), auditEvent.EntityId);
        Assert.Equal(PrescriptionAuditActions.Created, auditEvent.Action);
        Assert.Equal(UserId, auditEvent.UserId);
        Assert.Equal("Veterinarian", auditEvent.UserProfile);
        Assert.NotEqual(default, auditEvent.OccurredAt);
        Assert.NotNull(auditEvent.MetadataJson!);
        Assert.Contains("\"AttendanceId\":10", auditEvent.MetadataJson!);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWriteSafeAuditLogMetadata_WhenSuccessful()
    {
        var (uc, _, auditWriter) = CreateUseCase(AttendanceStatus.Open);

        var result = await uc.ExecuteAsync(10, ValidRequest(item: new(123, 2, " ml ", "SENSITIVE_DOSAGE", 7)), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        var auditEvent = Assert.Single(auditWriter.Events);
        var combined = string.Join(" ", auditEvent.EntityName, auditEvent.EntityId, auditEvent.Action, auditEvent.UserId, auditEvent.UserProfile, auditEvent.MetadataJson!);
        Assert.DoesNotContain("Notes", combined);
        Assert.DoesNotContain("notes", auditEvent.MetadataJson!);
        Assert.DoesNotContain("Dosage", combined);
        Assert.DoesNotContain("SENSITIVE_DOSAGE", combined);
        Assert.DoesNotContain("Items", combined);
        Assert.DoesNotContain("ProductId", combined);
        Assert.DoesNotContain("123", auditEvent.MetadataJson!);
    }

    [Theory][InlineData(0)][InlineData(-1)] public async Task ExecuteAsync_ShouldValidateRouteAttendanceId(long id) => await AssertValidation(id, ValidRequest());
    [Theory][InlineData(0)][InlineData(-1)] public async Task ExecuteAsync_ShouldValidateRequestAttendanceId(long id) => await AssertValidation(10, ValidRequest(id));
    [Fact] public async Task ExecuteAsync_ShouldValidateDivergentIds() => await AssertValidation(10, ValidRequest(11));
    [Fact] public async Task ExecuteAsync_ShouldValidateIssuedAt() => await AssertValidation(10, new CreatePrescriptionRequest(10, default, null, [new CreatePrescriptionItemRequest(1, 2, " ml ", " bid ", 7)]));
    [Fact] public async Task ExecuteAsync_ShouldValidateNullItems() => await AssertValidation(10, new CreatePrescriptionRequest(10, Now, null, null!));
    [Fact] public async Task ExecuteAsync_ShouldValidateEmptyItems() => await AssertValidation(10, new CreatePrescriptionRequest(10, Now, null, []));
    [Fact] public async Task ExecuteAsync_ShouldValidateQuantity() => await AssertValidation(10, ValidRequest(item: new(null, 0, "ml", "bid", null)));
    [Fact] public async Task ExecuteAsync_ShouldValidateUnit() => await AssertValidation(10, ValidRequest(item: new(null, 1, " ", "bid", null)));
    [Fact] public async Task ExecuteAsync_ShouldValidateDosage() => await AssertValidation(10, ValidRequest(item: new(null, 1, "ml", " ", null)));
    [Fact] public async Task ExecuteAsync_ShouldValidateDuration() => await AssertValidation(10, ValidRequest(item: new(null, 1, "ml", "bid", 0)));
    [Fact] public async Task ExecuteAsync_ShouldValidateProductId() => await AssertValidation(10, ValidRequest(item: new(0, 1, "ml", "bid", null)));

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WithoutRepositoryOrAudit_WhenAttendanceMissing()
    {
        var repo = new FakePrescriptionRepository();
        var audit = new FakeClinicalAuditLogWriter();
        var result = await new CreatePrescriptionUseCase(new FakeAttendanceRepository(), repo, new FakeCurrentUserService(UserId), audit).ExecuteAsync(10, ValidRequest(), CancellationToken.None);
        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal(0, repo.AddCallsCount);
        Assert.Equal(0, audit.WriteCallsCount);
    }

    [Theory][InlineData(AttendanceStatus.Closed)][InlineData(AttendanceStatus.Canceled)]
    public async Task ExecuteAsync_ShouldReturnConflict_WithoutRepositoryOrAudit_WhenAttendanceIsNotOpen(AttendanceStatus status)
    {
        var (uc, repo, audit) = CreateUseCase(status);
        var result = await uc.ExecuteAsync(10, ValidRequest(), CancellationToken.None);
        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal(0, repo.AddCallsCount);
        Assert.Equal(0, audit.WriteCallsCount);
    }

    [Fact]
    public void Constructor_ShouldRequireAuthorshipAndAuditServices()
    {
        var ctor = typeof(CreatePrescriptionUseCase).GetConstructors().Single();
        var names = ctor.GetParameters().Select(p => p.ParameterType.Name).ToArray();
        Assert.Contains("ICurrentUserService", names);
        Assert.Contains("IClinicalAuditLogWriter", names);
    }

    private static async Task AssertValidation(long routeId, CreatePrescriptionRequest request)
    {
        var repo = new FakePrescriptionRepository();
        var audit = new FakeClinicalAuditLogWriter();
        var result = await new CreatePrescriptionUseCase(new FakeAttendanceRepository(), repo, new FakeCurrentUserService(UserId), audit).ExecuteAsync(routeId, request, CancellationToken.None);
        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repo.AddCallsCount);
        Assert.Equal(0, audit.WriteCallsCount);
    }

    private static (CreatePrescriptionUseCase, FakePrescriptionRepository, FakeClinicalAuditLogWriter) CreateUseCase(AttendanceStatus status, FakeCurrentUserService? currentUserService = null)
    {
        var attendanceRepo = new FakeAttendanceRepository();
        var attendance = Attendance.Create(1, "ATT", Now, AttendanceType.Consultation, UserId, Now);
        if (status == AttendanceStatus.Closed) attendance.Close(Now.AddMinutes(1), UserId, Now.AddMinutes(1));
        if (status == AttendanceStatus.Canceled) attendance.Cancel(UserId, Now.AddMinutes(1));
        attendanceRepo.AddAttendanceForLookup(10, attendance);
        var repo = new FakePrescriptionRepository();
        var audit = new FakeClinicalAuditLogWriter();
        return (new CreatePrescriptionUseCase(attendanceRepo, repo, currentUserService ?? new FakeCurrentUserService(UserId), audit), repo, audit);
    }

    private static CreatePrescriptionRequest ValidRequest(long attendanceId = 10, DateTime? issuedAt = null, CreatePrescriptionItemRequest? item = null) => new(attendanceId, issuedAt ?? Now, " notes ", [item ?? new CreatePrescriptionItemRequest(1, 2, " ml ", " bid ", 7)]);
}
