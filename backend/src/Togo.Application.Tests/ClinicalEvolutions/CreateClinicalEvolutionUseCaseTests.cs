using Togo.Application.Auditing;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.ClinicalEvolutions.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.ClinicalEvolutions.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tests.Security.Fakes;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.ClinicalEvolutions;

public sealed class CreateClinicalEvolutionUseCaseTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RegisteredAt = new(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAttendanceExistsAndIsOpen()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var clinicalEvolutionRepository = new FakeClinicalEvolutionRepository();
        var useCase = CreateUseCase(attendanceRepository, clinicalEvolutionRepository);

        var result = await useCase.ExecuteAsync(10, CreateRequest(10), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.AttendanceId);
        Assert.Equal("Clinical note", result.Data.Text);
        Assert.Equal(1, clinicalEvolutionRepository.AddCallsCount);
        var persisted = Assert.Single(clinicalEvolutionRepository.Items);
        Assert.Equal(TestUserId, persisted.CreatedByUserId);
        Assert.Equal(TestUserId, persisted.UpdatedByUserId);
        Assert.True(persisted.CreatedAt >= TestCreatedAt);
        Assert.Equal(persisted.CreatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseCurrentUserAndWriteCreatedAuditLog_WhenSuccessful()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var clinicalEvolutionRepository = new FakeClinicalEvolutionRepository();
        var currentUserService = new FakeCurrentUserService(TestUserId)
        {
            CurrentUser = new CurrentUserInfo(TestUserId, Profile: "Veterinarian", IsAuthenticated: true)
        };
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var useCase = CreateUseCase(attendanceRepository, clinicalEvolutionRepository, currentUserService, auditLogWriter);

        var result = await useCase.ExecuteAsync(10, CreateRequest(10, text: "Sensitive clinical text"), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        var persisted = Assert.Single(clinicalEvolutionRepository.Items);
        Assert.Equal(TestUserId, persisted.CreatedByUserId);
        Assert.Equal(TestUserId, persisted.UpdatedByUserId);
        var auditEvent = Assert.Single(auditLogWriter.Events);
        Assert.Equal(nameof(ClinicalEvolution), auditEvent.EntityName);
        Assert.Equal(persisted.Id.ToString(), auditEvent.EntityId);
        Assert.Equal(ClinicalEvolutionAuditActions.Created, auditEvent.Action);
        Assert.Equal(TestUserId, auditEvent.UserId);
        Assert.Equal("Veterinarian", auditEvent.UserProfile);
        Assert.NotNull(auditEvent.MetadataJson);
        Assert.Contains("\"AttendanceId\":10", auditEvent.MetadataJson);
        Assert.Contains("\"Type\":\"ClinicalNote\"", auditEvent.MetadataJson);
        Assert.DoesNotContain("Text", auditEvent.MetadataJson);
        Assert.DoesNotContain("Sensitive clinical text", auditEvent.MetadataJson);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenRouteAttendanceIdIsInvalid(long attendanceId)
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await CreateUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(attendanceId, CreateRequest(10), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenRequestAttendanceIdIsInvalid(long requestAttendanceId)
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await CreateUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(10, CreateRequest(requestAttendanceId), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenRouteAndBodyAttendanceIdDiverge()
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await CreateUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(10, CreateRequest(11), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist()
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await CreateUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(10, CreateRequest(10), CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsClosed()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var attendance = CreateOpenAttendance();
        attendance.Close(RegisteredAt, TestUserId, RegisteredAt);
        attendanceRepository.AddAttendanceForLookup(10, attendance);
        var repository = new FakeClinicalEvolutionRepository();

        var result = await CreateUseCase(attendanceRepository, repository).ExecuteAsync(10, CreateRequest(10), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceIsCanceled()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var attendance = CreateOpenAttendance();
        attendance.Cancel(TestUserId, RegisteredAt);
        attendanceRepository.AddAttendanceForLookup(10, attendance);
        var repository = new FakeClinicalEvolutionRepository();

        var result = await CreateUseCase(attendanceRepository, repository).ExecuteAsync(10, CreateRequest(10), CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenRegisteredAtIsDefault()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var repository = new FakeClinicalEvolutionRepository();

        var result = await CreateUseCase(attendanceRepository, repository).ExecuteAsync(10, CreateRequest(10, registeredAt: default(DateTime)), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenTextIsEmpty()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var repository = new FakeClinicalEvolutionRepository();

        var result = await CreateUseCase(attendanceRepository, repository).ExecuteAsync(10, CreateRequest(10, text: "   "), CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowCurrentUserResolutionExceptionAndNotWriteAuditLog_WhenCurrentUserDoesNotResolve()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var repository = new FakeClinicalEvolutionRepository();
        var currentUserService = new FakeCurrentUserService(TestUserId) { ThrowResolutionException = true };
        var auditLogWriter = new FakeClinicalAuditLogWriter();

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() =>
            CreateUseCase(attendanceRepository, repository, currentUserService, auditLogWriter).ExecuteAsync(10, CreateRequest(10), CancellationToken.None));

        Assert.Equal(0, repository.AddCallsCount);
        Assert.Equal(0, auditLogWriter.WriteCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenTypeIsInvalid()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var repository = new FakeClinicalEvolutionRepository();
        var request = new CreateClinicalEvolutionRequest(10, RegisteredAt, (EvolutionType)999, "Clinical note");

        var result = await CreateUseCase(attendanceRepository, repository).ExecuteAsync(10, request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.AddCallsCount);
    }

    private static CreateClinicalEvolutionUseCase CreateUseCase(FakeAttendanceRepository attendanceRepository, FakeClinicalEvolutionRepository clinicalEvolutionRepository) =>
        CreateUseCase(attendanceRepository, clinicalEvolutionRepository, new FakeCurrentUserService(TestUserId), new FakeClinicalAuditLogWriter());

    private static CreateClinicalEvolutionUseCase CreateUseCase(
        FakeAttendanceRepository attendanceRepository,
        FakeClinicalEvolutionRepository clinicalEvolutionRepository,
        FakeCurrentUserService currentUserService,
        FakeClinicalAuditLogWriter clinicalAuditLogWriter) =>
        new(attendanceRepository, clinicalEvolutionRepository, currentUserService, clinicalAuditLogWriter, new TestLogger<CreateClinicalEvolutionUseCase>());

    private static Attendance CreateOpenAttendance() => Attendance.Create(1, 1, "ATT-001", RegisteredAt.AddHours(-1), AttendanceType.Consultation, TestUserId, TestCreatedAt);

    private static CreateClinicalEvolutionRequest CreateRequest(long attendanceId, DateTime? registeredAt = null, string text = "  Clinical note  ") =>
        new(attendanceId, registeredAt ?? RegisteredAt, EvolutionType.ClinicalNote, text);
}
