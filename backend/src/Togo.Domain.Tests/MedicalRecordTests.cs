using Togo.Domain.Entities;
using Xunit;

namespace Togo.Domain.Tests;

public class MedicalRecordTests
{
    [Fact]
    public void Create_ShouldCreateMedicalRecord_WhenDataIsValid()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var medicalRecord = MedicalRecord.Create(
            patientId: 10,
            generalNotes: "  Stable patient  ",
            flagsJson: "  {\"risk\":\"low\"}  ",
            createdByUserId: Guid.Parse("11111111-2222-3333-4444-555555555555"),
            createdAt: updatedAt);

        // Assert
        Assert.Equal(0, medicalRecord.Id);
        Assert.Equal(10, medicalRecord.PatientId);
        Assert.Equal("Stable patient", medicalRecord.GeneralNotes);
        Assert.Equal("{\"risk\":\"low\"}", medicalRecord.FlagsJson);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), medicalRecord.CreatedByUserId);
        Assert.Equal(updatedAt, medicalRecord.CreatedAt);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), medicalRecord.UpdatedByUserId);
        Assert.Equal(updatedAt, medicalRecord.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenPatientIdIsInvalid(long patientId)
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            MedicalRecord.Create(patientId, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt));
        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("patientId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedAtIsDefault()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), default));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldNormalizeGeneralNotes_WhenHasLeadingAndTrailingSpaces()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var medicalRecord = MedicalRecord.Create(10, "  needs check  ", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Assert
        Assert.Equal("needs check", medicalRecord.GeneralNotes);
    }

    [Fact]
    public void Create_ShouldNormalizeFlagsJson_WhenHasLeadingAndTrailingSpaces()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var medicalRecord = MedicalRecord.Create(10, "notes", "  {\"k\":1}  ", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Assert
        Assert.Equal("{\"k\":1}", medicalRecord.FlagsJson);
    }

    [Fact]
    public void Create_ShouldAllowNullOptionalFields()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var medicalRecord = MedicalRecord.Create(10, null, null, Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Assert
        Assert.Null(medicalRecord.GeneralNotes);
        Assert.Null(medicalRecord.FlagsJson);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldConvertEmptyOrWhitespaceOptionalFieldsToNull(string value)
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var medicalRecord = MedicalRecord.Create(10, value, value, Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Assert
        Assert.Null(medicalRecord.GeneralNotes);
        Assert.Null(medicalRecord.FlagsJson);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedByUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MedicalRecord.Create(10, "notes", "{}", Guid.Empty, DateTime.UtcNow));

        Assert.StartsWith("User id is required", exception.Message);
        Assert.Equal("createdByUserId", exception.ParamName);
    }

    [Fact]
    public void UpdateNotes_ShouldThrowArgumentException_WhenUpdatedByUserIdIsEmpty()
    {
        var medicalRecord = MedicalRecord.Create(10, "old", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);

        var exception = Assert.Throws<ArgumentException>(() => medicalRecord.UpdateNotes("new", "{}", Guid.Empty, DateTime.UtcNow));

        Assert.StartsWith("User id is required", exception.Message);
        Assert.Equal("updatedByUserId", exception.ParamName);
    }

    [Fact]
    public void UpdateNotes_ShouldUpdateFields_WhenDataIsValid()
    {
        // Arrange
        var initialUpdatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{\"old\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), initialUpdatedAt);

        // Act
        medicalRecord.UpdateNotes("new notes", "{\"new\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Equal("new notes", medicalRecord.GeneralNotes);
        Assert.Equal("{\"new\":1}", medicalRecord.FlagsJson);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), medicalRecord.CreatedByUserId);
        Assert.Equal(initialUpdatedAt, medicalRecord.CreatedAt);
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), medicalRecord.UpdatedByUserId);
        Assert.Equal(newUpdatedAt, medicalRecord.UpdatedAt);
    }

    [Fact]
    public void UpdateNotes_ShouldNormalizeGeneralNotes_WhenHasLeadingAndTrailingSpaces()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act
        medicalRecord.UpdateNotes("  trimmed notes  ", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Equal("trimmed notes", medicalRecord.GeneralNotes);
    }

    [Fact]
    public void UpdateNotes_ShouldNormalizeFlagsJson_WhenHasLeadingAndTrailingSpaces()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act
        medicalRecord.UpdateNotes("notes", "  {\"f\":true}  ", Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Equal("{\"f\":true}", medicalRecord.FlagsJson);
    }

    [Fact]
    public void UpdateNotes_ShouldAllowNullOptionalFields()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{\"old\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act
        medicalRecord.UpdateNotes(null, null, Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Null(medicalRecord.GeneralNotes);
        Assert.Null(medicalRecord.FlagsJson);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateNotes_ShouldConvertEmptyOrWhitespaceOptionalFieldsToNull(string value)
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{\"old\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act
        medicalRecord.UpdateNotes(value, value, Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Null(medicalRecord.GeneralNotes);
        Assert.Null(medicalRecord.FlagsJson);
    }

    [Fact]
    public void UpdateNotes_ShouldThrowArgumentException_WhenUpdatedAtIsDefault()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{\"old\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => medicalRecord.UpdateNotes("new", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), default));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("updatedAt", exception.ParamName);
    }

    [Fact]
    public void UpdateNotes_ShouldPreservePatientId()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);

        // Act
        medicalRecord.UpdateNotes("new", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Equal(10, medicalRecord.PatientId);
    }

    [Fact]
    public void UpdateNotes_ShouldPreserveId_WhenApplyingUpdates()
    {
        // Arrange
        var updatedAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var newUpdatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var medicalRecord = MedicalRecord.Create(10, "old", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), updatedAt);
        var initialId = medicalRecord.Id;

        // Act
        medicalRecord.UpdateNotes("new", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), newUpdatedAt);

        // Assert
        Assert.Equal(initialId, medicalRecord.Id);
    }

    [Fact]
    public void Create_ShouldInitializeSoftDeleteFieldsAsNotDeleted()
    {
        var medicalRecord = MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);

        Assert.False(medicalRecord.IsDeleted);
        Assert.Null(medicalRecord.DeletedAt);
        Assert.Null(medicalRecord.DeletedByUserId);
    }

    [Fact]
    public void SoftDelete_ShouldMarkRecordAsDeleted_WhenDataIsValid()
    {
        var createdAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var deletedAt = new DateTime(2026, 5, 23, 11, 0, 0, DateTimeKind.Utc);
        var createdByUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var deletedByUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var medicalRecord = MedicalRecord.Create(10, "notes", "{\"flag\":true}", createdByUserId, createdAt);

        medicalRecord.SoftDelete(deletedByUserId, deletedAt);

        Assert.True(medicalRecord.IsDeleted);
        Assert.Equal(deletedAt, medicalRecord.DeletedAt);
        Assert.Equal(deletedByUserId, medicalRecord.DeletedByUserId);
    }

    [Fact]
    public void SoftDelete_ShouldThrowArgumentException_WhenDeletedByUserIdIsEmpty()
    {
        var medicalRecord = MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);

        var exception = Assert.Throws<ArgumentException>(() => medicalRecord.SoftDelete(Guid.Empty, DateTime.UtcNow));

        Assert.StartsWith("User id is required", exception.Message);
        Assert.Equal("deletedByUserId", exception.ParamName);
    }

    [Fact]
    public void SoftDelete_ShouldThrowArgumentException_WhenDeletedAtIsDefault()
    {
        var medicalRecord = MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);

        var exception = Assert.Throws<ArgumentException>(() => medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), default));

        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("deletedAt", exception.ParamName);
    }

    [Fact]
    public void SoftDelete_ShouldThrowArgumentException_WhenDeletedAtIsNotUtc()
    {
        var medicalRecord = MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow);
        var localDeletedAt = new DateTime(2026, 5, 23, 11, 0, 0, DateTimeKind.Local);

        var exception = Assert.Throws<ArgumentException>(() => medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), localDeletedAt));

        Assert.StartsWith("Deleted at must be UTC", exception.Message);
        Assert.Equal("deletedAt", exception.ParamName);
    }

    [Fact]
    public void SoftDelete_ShouldPreserveClinicalAuthorshipTimestampsAndContent()
    {
        var createdAt = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var deletedAt = new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc);
        var createdByUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var updatedByUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var deletedByUserId = Guid.Parse("99999999-8888-7777-6666-555555555555");
        var medicalRecord = MedicalRecord.Create(10, "notes", "{\"flag\":true}", createdByUserId, createdAt);
        medicalRecord.UpdateNotes("updated notes", "{\"flag\":false}", updatedByUserId, updatedAt);

        medicalRecord.SoftDelete(deletedByUserId, deletedAt);

        Assert.Equal(createdAt, medicalRecord.CreatedAt);
        Assert.Equal(createdByUserId, medicalRecord.CreatedByUserId);
        Assert.Equal(updatedAt, medicalRecord.UpdatedAt);
        Assert.Equal(updatedByUserId, medicalRecord.UpdatedByUserId);
        Assert.Equal("updated notes", medicalRecord.GeneralNotes);
        Assert.Equal("{\"flag\":false}", medicalRecord.FlagsJson);
        Assert.Equal(10, medicalRecord.PatientId);
    }

    [Fact]
    public void SoftDelete_ShouldThrowInvalidOperationException_WhenRecordIsAlreadyDeleted()
    {
        var medicalRecord = MedicalRecord.Create(10, "notes", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddHours(-1));
        medicalRecord.SoftDelete(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            medicalRecord.SoftDelete(Guid.Parse("99999999-8888-7777-6666-555555555555"), DateTime.UtcNow.AddMinutes(1)));

        Assert.Equal("Medical record is already soft deleted.", exception.Message);
    }

}
