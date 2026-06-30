using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Repositories;

public class ClinicalEvolutionRepositoryTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime CreatedAt = new(2026, 6, 20, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public async Task AddAsync_ShouldPersistClinicalEvolution()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var repository = new ClinicalEvolutionRepository(context);
        var attendance = await AddAttendanceAsync(context, "ATT-CE-001");
        var evolution = ClinicalEvolution.Create(attendance.ClinicId, attendance.Id, new DateTime(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc), EvolutionType.ClinicalNote, "note", UserId, CreatedAt);

        await repository.AddAsync(evolution);

        var persisted = await context.ClinicalEvolutions.AsNoTracking().SingleAsync();
        Assert.Equal(attendance.ClinicId, persisted.ClinicId);
        Assert.Equal(attendance.Id, persisted.AttendanceId);
        Assert.Equal("note", persisted.Text);
        Assert.Equal(UserId, persisted.CreatedByUserId);
        Assert.Equal(CreatedAt, persisted.CreatedAt);
        Assert.Equal(UserId, persisted.UpdatedByUserId);
        Assert.Equal(CreatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task ListByAttendanceIdAsync_ShouldReturnOnlyRequestedAttendanceOrderedByRegisteredAtAndId()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var repository = new ClinicalEvolutionRepository(context);
        var attendance = await AddAttendanceAsync(context, "ATT-CE-002");
        var otherAttendance = await AddAttendanceAsync(context, "ATT-CE-003");
        var later = ClinicalEvolution.Create(attendance.ClinicId, attendance.Id, new DateTime(2026, 6, 20, 11, 0, 0, DateTimeKind.Utc), EvolutionType.ClinicalNote, "later", UserId, CreatedAt);
        var earlier = ClinicalEvolution.Create(attendance.ClinicId, attendance.Id, new DateTime(2026, 6, 20, 9, 0, 0, DateTimeKind.Utc), EvolutionType.ClinicalNote, "earlier", UserId, CreatedAt);
        var other = ClinicalEvolution.Create(otherAttendance.ClinicId, otherAttendance.Id, new DateTime(2026, 6, 20, 8, 0, 0, DateTimeKind.Utc), EvolutionType.ClinicalNote, "other", UserId, CreatedAt);
        context.ClinicalEvolutions.AddRange(later, earlier, other);
        await context.SaveChangesAsync();

        var result = await repository.ListByAttendanceIdAsync(attendance.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(["earlier", "later"], result.Select(item => item.Text).ToArray());
        Assert.All(result, item => Assert.Equal(attendance.ClinicId, item.ClinicId));
        Assert.All(result, item => Assert.Equal(attendance.Id, item.AttendanceId));
    }


    [Fact]
    public void ClinicalEvolutionConfiguration_ShouldConfigureClinicIdRequiredRelationshipAndIndexes()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        var entityType = context.Model.FindEntityType(typeof(ClinicalEvolution));
        Assert.NotNull(entityType);

        var clinicIdProperty = entityType!.FindProperty(nameof(ClinicalEvolution.ClinicId));
        Assert.NotNull(clinicIdProperty);
        Assert.False(clinicIdProperty!.IsNullable);

        var clinicForeignKey = Assert.Single(entityType.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Count == 1 && foreignKey.Properties[0].Name == nameof(ClinicalEvolution.ClinicId));
        Assert.Equal(DeleteBehavior.Restrict, clinicForeignKey.DeleteBehavior);
        Assert.Equal(typeof(Clinic), clinicForeignKey.PrincipalEntityType.ClrType);

        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Count == 1 && index.Properties[0].Name == nameof(ClinicalEvolution.ClinicId));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(ClinicalEvolution.ClinicId), nameof(ClinicalEvolution.AttendanceId)]));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(ClinicalEvolution.ClinicId), nameof(ClinicalEvolution.RegisteredAt)]));
    }

    private static async Task<Attendance> AddAttendanceAsync(Togo.Infrastructure.Persistence.AppDbContext context, string attendanceNumber)
    {
        var clinic = await ClinicalScopeTestData.EnsureClinicAsync(context);
        var patient = Patient.Create(clinic.Id, PatientType.Pet, attendanceNumber + " Patient", null, "Active", DateTime.UtcNow);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var attendance = Attendance.Create(patient.ClinicId, patient.Id, attendanceNumber, DateTime.UtcNow, AttendanceType.Consultation, Guid.Parse("11111111-1111-1111-1111-111111111111"), DateTime.UtcNow);
        context.Attendances.Add(attendance);
        await context.SaveChangesAsync();
        return attendance;
    }
}
