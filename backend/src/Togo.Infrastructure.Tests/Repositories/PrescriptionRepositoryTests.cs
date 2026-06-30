using Microsoft.EntityFrameworkCore;
using Togo.Application.Prescriptions.Repositories;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Repositories;

public class PrescriptionRepositoryTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact] public async Task AddAsync_ShouldPersistPrescriptionAndItemsLinkedToPrescriptionId() { using var context = SqliteAppDbContextFactory.CreateContext(out var connection); await using var _ = connection; var attendance = await AddAttendanceAsync(context, "ATT-P-001"); var prescription = Prescription.Create(attendance.ClinicId, attendance.Id, DateTime.UtcNow, "notes"); var repo = new PrescriptionRepository(context); await repo.AddAsync(prescription, [new PrescriptionItemDraft(10, 2, "ml", "bid", 7), new PrescriptionItemDraft(null, 1, "tab", "sid", null)]); var persisted = await context.Prescriptions.AsNoTracking().SingleAsync(); var items = await context.PrescriptionItems.AsNoTracking().ToListAsync(); Assert.Equal(attendance.ClinicId, persisted.ClinicId); Assert.Equal(attendance.Id, persisted.AttendanceId); Assert.Equal(2, items.Count); Assert.All(items, item => Assert.Equal(prescription.Id, item.PrescriptionId)); }

    [Fact] public async Task ListByAttendanceIdAsync_ShouldFilterOrderAndCountItems() { using var context = SqliteAppDbContextFactory.CreateContext(out var connection); await using var _ = connection; var attendance = await AddAttendanceAsync(context, "ATT-P-002"); var otherAttendance = await AddAttendanceAsync(context, "ATT-P-003"); var repo = new PrescriptionRepository(context); var later = Prescription.Create(attendance.ClinicId, attendance.Id, new DateTime(2026,6,24,11,0,0,DateTimeKind.Utc), null); var earlier = Prescription.Create(attendance.ClinicId, attendance.Id, new DateTime(2026,6,24,9,0,0,DateTimeKind.Utc), null); var other = Prescription.Create(otherAttendance.ClinicId, otherAttendance.Id, new DateTime(2026,6,24,8,0,0,DateTimeKind.Utc), null); await repo.AddAsync(later, [new PrescriptionItemDraft(null, 1, "ml", "later", null)]); await repo.AddAsync(earlier, [new PrescriptionItemDraft(null, 1, "ml", "earlier1", null), new PrescriptionItemDraft(null, 2, "ml", "earlier2", null)]); await repo.AddAsync(other, [new PrescriptionItemDraft(null, 1, "ml", "other", null)]); var result = await repo.ListByAttendanceIdAsync(attendance.Id); Assert.Equal([earlier.Id, later.Id], result.Select(x => x.Id).ToArray()); Assert.Equal([2, 1], result.Select(x => x.ItemCount).ToArray()); Assert.All(result, item => Assert.Equal(attendance.Id, item.AttendanceId)); }

    [Fact]
    public void PrescriptionConfiguration_ShouldConfigureClinicIdRequiredRelationshipAndIndexes()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        var entityType = context.Model.FindEntityType(typeof(Prescription));
        var clinicIdProperty = entityType!.FindProperty(nameof(Prescription.ClinicId));

        Assert.NotNull(clinicIdProperty);
        Assert.False(clinicIdProperty!.IsNullable);
        var clinicForeignKey = Assert.Single(entityType.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Count == 1 && foreignKey.Properties[0].Name == nameof(Prescription.ClinicId));
        Assert.Equal(typeof(Clinic), clinicForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, clinicForeignKey.DeleteBehavior);
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Count == 1 && index.Properties[0].Name == nameof(Prescription.ClinicId));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Prescription.ClinicId), nameof(Prescription.AttendanceId)]));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Prescription.ClinicId), nameof(Prescription.IssuedAt)]));

        var itemEntityType = context.Model.FindEntityType(typeof(PrescriptionItem));
        Assert.Null(itemEntityType!.FindProperty("ClinicId"));
    }

    private static async Task<Attendance> AddAttendanceAsync(AppDbContext context, string attendanceNumber)
    {
        var clinic = await ClinicalScopeTestData.EnsureClinicAsync(context);
        var patient = Patient.Create(clinic.Id, PatientType.Pet, attendanceNumber + " Patient", null, "Active", DateTime.UtcNow);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var attendance = Attendance.Create(patient.ClinicId, patient.Id, attendanceNumber, DateTime.UtcNow, AttendanceType.Consultation, UserId, DateTime.UtcNow);
        context.Attendances.Add(attendance);
        await context.SaveChangesAsync();
        return attendance;
    }
}
