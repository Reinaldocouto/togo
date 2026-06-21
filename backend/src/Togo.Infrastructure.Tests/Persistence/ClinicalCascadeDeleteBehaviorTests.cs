using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Persistence;

public class ClinicalCascadeDeleteBehaviorTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ClinicalUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid DeletingUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void ClinicalRelationships_ShouldExposeReviewedDeleteBehaviorsInModel()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        AssertDeleteBehavior<Attendance>(context, nameof(Attendance.PatientId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<MedicalRecord>(context, nameof(MedicalRecord.PatientId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<ClinicalEvolution>(context, nameof(ClinicalEvolution.AttendanceId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<Prescription>(context, nameof(Prescription.AttendanceId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<PrescriptionItem>(context, nameof(PrescriptionItem.PrescriptionId), DeleteBehavior.Cascade);
    }

    [Fact]
    public async Task RemovingPatientWithMedicalRecord_ShouldBeBlockedByForeignKeyAndKeepMedicalRecord()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var patient = await AddPatientAsync(context, "Patient with protected medical record");
        var medicalRecord = MedicalRecord.Create(
            patient.Id,
            "Clinical history must survive principal physical delete attempts",
            "{\"protected\":true}",
            ClinicalUserId,
            new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc));

        context.MedicalRecords.Add(medicalRecord);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Patients.Remove(patient);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.Id == medicalRecord.Id));
    }

    [Fact]
    public async Task RemovingPatientWithAttendanceAndMedicalRecord_ShouldNotCascadeDeleteMedicalRecord()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var patient = await AddPatientAsync(context, "Patient with attendance and medical record");
        var attendance = Attendance.Create(patient.Id, "ATD-CASCADE-001", new DateTime(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var medicalRecord = MedicalRecord.Create(
            patient.Id,
            "Medical record must remain when patient delete is blocked",
            null,
            ClinicalUserId,
            new DateTime(2026, 6, 5, 8, 30, 0, DateTimeKind.Utc));

        context.Attendances.Add(attendance);
        context.MedicalRecords.Add(medicalRecord);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Patients.Remove(patient);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.Id == medicalRecord.Id));
        Assert.Equal(1, await context.Attendances.AsNoTracking().CountAsync(item => item.Id == attendance.Id));
    }

    [Fact]
    public async Task RemovingAttendanceWithClinicalEvolution_ShouldBeBlockedByForeignKeyAndKeepEvolution()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var attendance = await AddAttendanceAsync(context, "Patient protected evolution", "ATD-CASCADE-002");
        var evolution = ClinicalEvolution.Create(
            attendance.Id,
            new DateTime(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc),
            EvolutionType.ClinicalNote,
            "Evolution must not be physically deleted through Attendance cascade.",
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc));

        context.ClinicalEvolutions.Add(evolution);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Attendances.Remove(attendance);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(1, await context.ClinicalEvolutions.AsNoTracking().CountAsync(item => item.Id == evolution.Id));
    }

    [Fact]
    public async Task RemovingAttendanceWithPrescription_ShouldBeBlockedByForeignKeyAndKeepPrescriptionAndItems()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var attendance = await AddAttendanceAsync(context, "Patient protected prescription", "ATD-CASCADE-003");
        var prescription = Prescription.Create(attendance.Id, new DateTime(2026, 6, 5, 9, 30, 0, DateTimeKind.Utc), "Prescription must remain.");
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        var item = PrescriptionItem.Create(prescription.Id, null, 1.5m, "ml", "Every 12 hours", 7);
        context.PrescriptionItems.Add(item);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Attendances.Remove(attendance);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(1, await context.Prescriptions.AsNoTracking().CountAsync(entity => entity.Id == prescription.Id));
        Assert.Equal(1, await context.PrescriptionItems.AsNoTracking().CountAsync(entity => entity.Id == item.Id));
    }

    [Fact]
    public async Task SoftDeletedMedicalRecord_ShouldRemainPersistedAndBeIgnoredByDefaultRepositoryQueries()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient soft delete after cascade review");
        var medicalRecord = MedicalRecord.Create(
            patient.Id,
            "Soft deleted record must remain persisted.",
            null,
            ClinicalUserId,
            new DateTime(2026, 6, 5, 11, 0, 0, DateTimeKind.Utc));
        await repository.AddAsync(medicalRecord, CancellationToken.None);

        medicalRecord.SoftDelete(DeletingUserId, new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc));
        await repository.UpdateAsync(medicalRecord, CancellationToken.None);

        Assert.Null(await repository.GetByIdAsync(medicalRecord.Id, CancellationToken.None));
        Assert.Null(await repository.GetByPatientIdAsync(patient.Id, CancellationToken.None));
        Assert.False(await repository.ExistsByPatientIdAsync(patient.Id, CancellationToken.None));
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.Id == medicalRecord.Id && record.IsDeleted));
    }

    private static void AssertDeleteBehavior<TEntity>(AppDbContext context, string foreignKeyPropertyName, DeleteBehavior expected)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entityType);

        var foreignKey = entityType!.GetForeignKeys().Single(key => key.Properties.Any(property => property.Name == foreignKeyPropertyName));
        Assert.Equal(expected, foreignKey.DeleteBehavior);
    }

    private static async Task<Attendance> AddAttendanceAsync(AppDbContext context, string patientName, string attendanceNumber)
    {
        var patient = await AddPatientAsync(context, patientName);
        var attendance = Attendance.Create(patient.Id, attendanceNumber, new DateTime(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc), AttendanceType.Consultation, TestUserId, TestCreatedAt);
        context.Attendances.Add(attendance);
        await context.SaveChangesAsync();
        return attendance;
    }

    private static async Task<Patient> AddPatientAsync(AppDbContext context, string name)
    {
        var patient = Patient.Create(PatientType.Pet, name, new DateOnly(2021, 1, 1), "Active", DateTime.UtcNow);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }
}
