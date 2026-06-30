using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Repositories;

public class AttendanceRepositoryTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public async Task AddAsync_ShouldPersistAttendance_WhenAttendanceIsValid()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Add");
        var openedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(patient.ClinicId, patient.Id, "ATD-001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);

        await repository.AddAsync(attendance);

        var persisted = await context.Attendances.AsNoTracking().SingleAsync();
        Assert.Equal("ATD-001", persisted.AttendanceNumber);
        Assert.Equal(patient.Id, persisted.PatientId);
        Assert.Equal(openedAt, persisted.OpenedAt);
        Assert.Equal(AttendanceStatus.Open, persisted.Status);
        Assert.Equal(AttendanceType.Consultation, persisted.Type);
        Assert.Null(persisted.ClosedAt);
        Assert.Equal(TestUserId, persisted.CreatedByUserId);
        Assert.Equal(TestCreatedAt, persisted.CreatedAt);
        Assert.Equal(TestUserId, persisted.UpdatedByUserId);
        Assert.Equal(TestCreatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAttendance_WhenAttendanceExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Get");
        var attendance = Attendance.Create(patient.ClinicId, patient.Id, "ATD-002", new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Emergency, TestUserId, TestCreatedAt);
        await repository.AddAsync(attendance);

        var result = await repository.GetByIdAsync(attendance.Id);

        Assert.NotNull(result);
        Assert.Equal(attendance.Id, result!.Id);
        Assert.Equal("ATD-002", result.AttendanceNumber);
        Assert.Equal(patient.Id, result.PatientId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAttendanceDoesNotExist()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);

        var result = await repository.GetByIdAsync(99999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAttendancesOrderedByOpenedAtDescendingThenIdDescending()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient List");

        var older = Attendance.Create(patient.ClinicId, patient.Id, "ATD-003", new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Return, TestUserId, TestCreatedAt);
        var newerA = Attendance.Create(patient.ClinicId, patient.Id, "ATD-004", new DateTime(2026, 5, 2, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Return, TestUserId, TestCreatedAt);
        var newerB = Attendance.Create(patient.ClinicId, patient.Id, "ATD-005", new DateTime(2026, 5, 2, 9, 0, 0, DateTimeKind.Utc), AttendanceType.Procedure, TestUserId, TestCreatedAt);

        await repository.AddAsync(older);
        await repository.AddAsync(newerA);
        await repository.AddAsync(newerB);

        var result = await repository.ListAsync();

        Assert.Equal(3, result.Count);
        var expected = new[] { newerB.Id, newerA.Id, older.Id };
        Assert.Equal(expected, result.Select(a => a.Id));
    }

    [Fact]
    public async Task ListByPatientIdAsync_ShouldReturnOnlyAttendancesForPatient()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patientA = await AddPatientAsync(context, "Patient A");
        var patientB = await AddPatientAsync(context, "Patient B");

        var a1 = Attendance.Create(patientA.ClinicId, patientA.Id, "ATD-006", new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Other, TestUserId, TestCreatedAt);
        var a2 = Attendance.Create(patientA.ClinicId, patientA.Id, "ATD-007", new DateTime(2026, 5, 2, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Exam, TestUserId, TestCreatedAt);
        var b1 = Attendance.Create(patientB.ClinicId, patientB.Id, "ATD-008", new DateTime(2026, 5, 3, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Emergency, TestUserId, TestCreatedAt);

        await repository.AddAsync(a1);
        await repository.AddAsync(a2);
        await repository.AddAsync(b1);

        var result = await repository.ListByPatientIdAsync(patientA.Id);

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(patientA.Id, item.PatientId));
        Assert.Equal(new[] { a2.Id, a1.Id }, result.Select(a => a.Id));
    }

    [Fact]
    public async Task ExistsByAttendanceNumberAsync_ShouldReturnTrue_WhenAttendanceNumberExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Exists");
        await repository.AddAsync(Attendance.Create(patient.ClinicId, patient.Id, "ATD-009", new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt));

        var exists = await repository.ExistsByAttendanceNumberAsync("ATD-009");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByAttendanceNumberAsync_ShouldReturnFalse_WhenAttendanceNumberDoesNotExist()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);

        var exists = await repository.ExistsByAttendanceNumberAsync("ATD-999");

        Assert.False(exists);
    }

    [Fact]
    public async Task HasOpenAttendanceForPatientAsync_ShouldReturnTrue_WhenPatientHasOpenAttendance()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Open");
        await repository.AddAsync(Attendance.Create(patient.ClinicId, patient.Id, "ATD-010", new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt));

        var hasOpen = await repository.HasOpenAttendanceForPatientAsync(patient.Id);

        Assert.True(hasOpen);
    }

    [Fact]
    public async Task HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasNoOpenAttendance()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient No Attendance");

        var hasOpen = await repository.HasOpenAttendanceForPatientAsync(patient.Id);

        Assert.False(hasOpen);
    }

    [Fact]
    public async Task HasOpenAttendanceForPatientAsync_ShouldReturnFalse_WhenPatientHasOnlyClosedOrCanceledAttendance()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Closed Canceled");

        var closed = Attendance.Create(patient.ClinicId, patient.Id, "ATD-011", new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        closed.Close(new DateTime(2026, 5, 1, 11, 0, 0, DateTimeKind.Utc), TestUserId, TestCreatedAt.AddHours(1));

        var canceled = Attendance.Create(patient.ClinicId, patient.Id, "ATD-012", new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        canceled.Cancel(TestUserId, TestCreatedAt.AddHours(1));

        await repository.AddAsync(closed);
        await repository.AddAsync(canceled);

        var hasOpen = await repository.HasOpenAttendanceForPatientAsync(patient.Id);

        Assert.False(hasOpen);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistClosedAttendance_WhenAttendanceIsClosed()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Update Closed");

        var attendance = Attendance.Create(patient.ClinicId, patient.Id, "ATD-013", new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        await repository.AddAsync(attendance);

        attendance.Close(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc), TestUserId, TestCreatedAt.AddHours(1));
        await repository.UpdateAsync(attendance);

        var persisted = await repository.GetByIdAsync(attendance.Id);
        Assert.NotNull(persisted);
        Assert.Equal(AttendanceStatus.Closed, persisted!.Status);
        Assert.NotNull(persisted.ClosedAt);
        Assert.Equal(TestUserId, persisted.ClosedByUserId);
        Assert.Equal(TestUserId, persisted.UpdatedByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), persisted.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistCanceledAttendance_WhenAttendanceIsCanceled()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new AttendanceRepository(context);
        var patient = await AddPatientAsync(context, "Patient Update Canceled");

        var attendance = Attendance.Create(patient.ClinicId, patient.Id, "ATD-014", new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        await repository.AddAsync(attendance);

        attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1));
        await repository.UpdateAsync(attendance);

        var persisted = await repository.GetByIdAsync(attendance.Id);
        Assert.NotNull(persisted);
        Assert.Equal(AttendanceStatus.Canceled, persisted!.Status);
        Assert.Null(persisted.ClosedAt);
        Assert.Equal(TestUserId, persisted.CanceledByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), persisted.CanceledAt);
        Assert.Equal(TestUserId, persisted.UpdatedByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), persisted.UpdatedAt);
    }

    private static async Task<Patient> AddPatientAsync(AppDbContext context, string name)
    {
        var clinic = await ClinicalScopeTestData.EnsureClinicAsync(context);
        var patient = Patient.Create(clinic.Id, PatientType.Pet, name, new DateOnly(2020, 1, 1), "Active", DateTime.UtcNow);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }
}
