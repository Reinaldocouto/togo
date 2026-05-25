using Microsoft.EntityFrameworkCore;
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
        var medicalRecord = MedicalRecord.Create(patient.Id, "Initial notes", "{\"allergies\":[\"none\"]}", updatedAt);

        await repository.AddAsync(medicalRecord);

        var persisted = await context.MedicalRecords.AsNoTracking().SingleAsync();
        Assert.Equal(patient.Id, persisted.PatientId);
        Assert.Equal("Initial notes", persisted.GeneralNotes);
        Assert.Equal("{\"allergies\":[\"none\"]}", persisted.FlagsJson);
        Assert.Equal(updatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient GetById MedicalRecord");
        var updatedAt = new DateTime(2026, 5, 11, 9, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(patient.Id, "Notes by id", "{\"risk\":false}", updatedAt);
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
    public async Task GetByPatientIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient GetByPatientId MedicalRecord");
        var medicalRecord = MedicalRecord.Create(
            patient.Id,
            "Notes by patient",
            "{\"vaccination\":\"up-to-date\"}",
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
    public async Task ExistsByPatientIdAsync_ShouldReturnTrue_WhenExists()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Exists MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Exists note", "{\"critical\":false}", DateTime.UtcNow);
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
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient Update MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "Old notes", "{\"risk\":false}", new DateTime(2026, 5, 13, 8, 0, 0, DateTimeKind.Utc));
        await repository.AddAsync(medicalRecord);

        var updatedAt = new DateTime(2026, 5, 13, 9, 30, 0, DateTimeKind.Utc);
        medicalRecord.UpdateNotes("Updated notes", "{\"risk\":true}", updatedAt);

        await repository.UpdateAsync(medicalRecord);

        var persisted = await context.MedicalRecords.AsNoTracking().SingleAsync(record => record.Id == medicalRecord.Id);
        Assert.Equal("Updated notes", persisted.GeneralNotes);
        Assert.Equal("{\"risk\":true}", persisted.FlagsJson);
        Assert.Equal(updatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task ReadMethods_ShouldUseAsNoTracking()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;

        var repository = new MedicalRecordRepository(context);
        var patient = await AddPatientAsync(context, "Patient AsNoTracking MedicalRecord");
        var medicalRecord = MedicalRecord.Create(patient.Id, "AsNoTracking notes", "{\"watch\":true}", new DateTime(2026, 5, 14, 11, 0, 0, DateTimeKind.Utc));
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
