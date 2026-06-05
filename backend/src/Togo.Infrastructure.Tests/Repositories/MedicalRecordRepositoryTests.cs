using Microsoft.EntityFrameworkCore;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Repositories;

public class MedicalRecordRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistMedicalRecord()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Add MedicalRecord");
        var updatedAt = new DateTime(2026, 5, 10, 10, 30, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(patient.Id, "Initial notes", "{\"allergies\":[\"none\"]}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        await repository.AddAsync(medicalRecord);

        var persisted = await context.MedicalRecords.AsNoTracking().SingleAsync();
        Assert.Equal(patient.Id, persisted.PatientId);
        Assert.Equal("Initial notes", persisted.GeneralNotes);
        Assert.Equal("{\"allergies\":[\"none\"]}", persisted.FlagsJson);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), persisted.CreatedByUserId);
        Assert.Equal(updatedAt, persisted.CreatedAt);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), persisted.UpdatedByUserId);
        Assert.Equal(updatedAt, persisted.UpdatedAt);
        Assert.False(persisted.IsDeleted);
        Assert.Null(persisted.DeletedAt);
        Assert.Null(persisted.DeletedByUserId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient GetById MedicalRecord");
        var updatedAt = new DateTime(2026, 5, 11, 9, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(patient.Id, "Notes by id", "{\"risk\":false}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);
        await repository.AddAsync(medicalRecord);

        var result = await repository.GetByIdAsync(medicalRecord.Id);

        Assert.NotNull(result);
        Assert.Equal(medicalRecord.Id, result!.Id);
        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal("Notes by id", result.GeneralNotes);
        Assert.Equal("{\"risk\":false}", result.FlagsJson);
        Assert.Equal(updatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);

        var result = await repository.GetByIdAsync(99999);

        Assert.Null(result);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMedicalRecordIsSoftDeleted()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Deleted GetById MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Deleted by id", "{\"deleted\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-2));
        await repository.AddAsync(medicalRecord);
        medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow.AddHours(-1));
        await repository.UpdateAsync(medicalRecord);

        var result = await repository.GetByIdAsync(medicalRecord.Id);

        Assert.Null(result);
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.Id == medicalRecord.Id));
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient GetByPatientId MedicalRecord");
        var medicalRecord = MedicalRecord.Create(
            patient.Id,
            "Notes by patient",
            "{\"vaccination\":\"up-to-date\"}", Guid.Parse("11111111-2222-3333-4444-555555555555"),
            new DateTime(2026, 5, 12, 12, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(medicalRecord);

        var result = await repository.GetByPatientIdAsync(patient.Id);

        Assert.NotNull(result);
        Assert.Equal(medicalRecord.Id, result!.Id);
        Assert.Equal(patient.Id, result.PatientId);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);

        var result = await repository.GetByPatientIdAsync(99999);

        Assert.Null(result);
    }


    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnNull_WhenMedicalRecordIsSoftDeleted()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Deleted GetByPatientId MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Deleted by patient", "{\"deleted\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-2));
        await repository.AddAsync(medicalRecord);
        medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow.AddHours(-1));
        await repository.UpdateAsync(medicalRecord);

        var result = await repository.GetByPatientIdAsync(patient.Id);

        Assert.Null(result);
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.PatientId == patient.Id));
    }

    [Fact]
    public async Task ExistsByPatientIdAsync_ShouldReturnTrue_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Exists MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Exists note", "{\"critical\":false}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);
        await repository.AddAsync(medicalRecord);

        var exists = await repository.ExistsByPatientIdAsync(patient.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByPatientIdAsync_ShouldReturnFalse_WhenNotFound()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);

        var exists = await repository.ExistsByPatientIdAsync(88888);

        Assert.False(exists);
    }


    [Fact]
    public async Task ExistsByPatientIdAsync_ShouldReturnFalse_WhenMedicalRecordIsSoftDeleted()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Deleted Exists MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Deleted exists", "{\"deleted\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-2));
        await repository.AddAsync(medicalRecord);
        medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow.AddHours(-1));
        await repository.UpdateAsync(medicalRecord);

        var exists = await repository.ExistsByPatientIdAsync(patient.Id);

        Assert.False(exists);
        Assert.Equal(1, await context.MedicalRecords.AsNoTracking().CountAsync(record => record.PatientId == patient.Id));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Update MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Old notes", "{\"risk\":false}", Guid.Parse("11111111-2222-3333-4444-555555555555"), new DateTime(2026, 5, 13, 8, 0, 0, DateTimeKind.Utc));
        await repository.AddAsync(medicalRecord);

        var updatedAt = new DateTime(2026, 5, 13, 9, 30, 0, DateTimeKind.Utc);
        medicalRecord.UpdateNotes("Updated notes", "{\"risk\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        await repository.UpdateAsync(medicalRecord);

        var persisted = await context.MedicalRecords.AsNoTracking().SingleAsync(record => record.Id == medicalRecord.Id);
        Assert.Equal("Updated notes", persisted.GeneralNotes);
        Assert.Equal("{\"risk\":true}", persisted.FlagsJson);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), persisted.CreatedByUserId);
        Assert.Equal(new DateTime(2026, 5, 13, 8, 0, 0, DateTimeKind.Utc), persisted.CreatedAt);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), persisted.UpdatedByUserId);
        Assert.Equal(updatedAt, persisted.UpdatedAt);
    }



    [Fact]
    public async Task UpdateAsync_ShouldPersistSoftDeleteFields()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient SoftDelete MedicalRecord");
        var createdAt = new DateTime(2026, 5, 15, 8, 0, 0, DateTimeKind.Utc);
        var deletedAt = new DateTime(2026, 5, 15, 9, 0, 0, DateTimeKind.Utc);
        var creatorUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var deletingUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Soft delete notes", "{\"risk\":false}", creatorUserId, createdAt);
        await repository.AddAsync(medicalRecord);

        medicalRecord.SoftDelete(deletingUserId, deletedAt);
        await repository.UpdateAsync(medicalRecord);

        var persisted = await context.MedicalRecords.AsNoTracking().SingleAsync(record => record.Id == medicalRecord.Id);
        Assert.True(persisted.IsDeleted);
        Assert.Equal(deletedAt, persisted.DeletedAt);
        Assert.Equal(deletingUserId, persisted.DeletedByUserId);
        Assert.Equal("Soft delete notes", persisted.GeneralNotes);
        Assert.Equal("{\"risk\":false}", persisted.FlagsJson);
        Assert.Equal(creatorUserId, persisted.CreatedByUserId);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.Equal(creatorUserId, persisted.UpdatedByUserId);
        Assert.Equal(createdAt, persisted.UpdatedAt);
    }

    [Fact]
    public void MedicalRecordConfiguration_ShouldConfigureSoftDeleteColumnsAsMinimalNullableSchema()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        var entityType = context.Model.FindEntityType(typeof(MedicalRecord));
        Assert.NotNull(entityType);

        var isDeleted = entityType!.FindProperty(nameof(MedicalRecord.IsDeleted));
        Assert.NotNull(isDeleted);
        Assert.False(isDeleted!.IsNullable);
        Assert.Equal(false, isDeleted.GetDefaultValue());

        var deletedAt = entityType.FindProperty(nameof(MedicalRecord.DeletedAt));
        Assert.NotNull(deletedAt);
        Assert.True(deletedAt!.IsNullable);

        var deletedByUserId = entityType.FindProperty(nameof(MedicalRecord.DeletedByUserId));
        Assert.NotNull(deletedByUserId);
        Assert.True(deletedByUserId!.IsNullable);
    }


    [Fact]
    public void RepositoryContract_ShouldNotExposePhysicalDeleteMethod()
    {
        var methodNames = typeof(IMedicalRecordRepository).GetMethods().Select(method => method.Name);

        Assert.DoesNotContain(methodNames, methodName => methodName.Contains("Delete", StringComparison.OrdinalIgnoreCase) && !methodName.Contains("Soft", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReadMethods_ShouldUseAsNoTracking()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient AsNoTracking MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "AsNoTracking notes", "{\"watch\":true}", Guid.Parse("11111111-2222-3333-4444-555555555555"), new DateTime(2026, 5, 14, 11, 0, 0, DateTimeKind.Utc));
        await repository.AddAsync(medicalRecord);

        context.ChangeTracker.Clear();

        var byId = await repository.GetByIdAsync(medicalRecord.Id);
        Assert.NotNull(byId);
        Assert.Empty(context.ChangeTracker.Entries<MedicalRecord>());

        var byPatientId = await repository.GetByPatientIdAsync(patient.Id);
        Assert.NotNull(byPatientId);
        Assert.Empty(context.ChangeTracker.Entries<MedicalRecord>());
    }

    private static async Task<Patient> AddPatientAsync(AppDbContext context, string name)
    {
        var patient = Patient.Create(PatientType.Pet, name, new DateOnly(2021, 1, 1), "Active", DateTime.UtcNow);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }
}
