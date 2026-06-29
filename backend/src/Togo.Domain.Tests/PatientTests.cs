using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Xunit;

namespace Togo.Domain.Tests;

public class PatientTests
{
    [Fact]
    public void Create_ShouldCreatePatient_WhenDataIsValid()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var birthDate = new DateOnly(2021, 5, 10);

        // Act
        var patient = Patient.Create(1, PatientType.Pet, "  Thor  ", birthDate, "  Active  ", createdAt);

        // Assert
        Assert.Equal(1, patient.ClinicId);
        Assert.Equal(PatientType.Pet, patient.Type);
        Assert.Equal("Thor", patient.Name);
        Assert.Equal(birthDate, patient.BirthDate);
        Assert.Equal("Active", patient.Status);
        Assert.Equal(createdAt, patient.CreatedAt);
        Assert.Null(patient.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldCreatePatient_WhenBirthDateIsNull()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var patient = Patient.Create(1, PatientType.Pet, "Thor", null, "Active", createdAt);

        // Assert
        Assert.Equal(PatientType.Pet, patient.Type);
        Assert.Equal("Thor", patient.Name);
        Assert.Null(patient.BirthDate);
        Assert.Equal("Active", patient.Status);
        Assert.Equal(createdAt, patient.CreatedAt);
        Assert.Null(patient.UpdatedAt);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenClinicIdIsInvalid(long clinicId)
    {
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Patient.Create(clinicId, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt));

        Assert.Equal("clinicId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Patient.Create(1, PatientType.Pet, "   ", new DateOnly(2021, 5, 10), "Active", createdAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenStatusIsEmpty()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "   ", createdAt));
        Assert.StartsWith("Status is required", exception.Message);
        Assert.Equal("status", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedAtIsDefault()
    {
        // Arrange
        var createdAt = default(DateTime);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void Update_ShouldUpdatePatient_WhenDataIsValid()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 5, 12, 10, 0, 0, DateTimeKind.Utc);
        var newBirthDate = new DateOnly(2022, 6, 15);
        var patient = Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt);

        // Act
        patient.Update("  Loki  ", newBirthDate, "  Inactive  ", updatedAt);

        // Assert
        Assert.Equal(1, patient.ClinicId);
        Assert.Equal(PatientType.Pet, patient.Type);
        Assert.Equal("Loki", patient.Name);
        Assert.Equal(newBirthDate, patient.BirthDate);
        Assert.Equal("Inactive", patient.Status);
        Assert.Equal(createdAt, patient.CreatedAt);
        Assert.Equal(updatedAt, patient.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldAllowNullBirthDate()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 5, 12, 10, 0, 0, DateTimeKind.Utc);
        var patient = Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt);

        // Act
        patient.Update("Thor", null, "Active", updatedAt);

        // Assert
        Assert.Null(patient.BirthDate);
        Assert.Equal(updatedAt, patient.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 5, 12, 10, 0, 0, DateTimeKind.Utc);
        var patient = Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            patient.Update("   ", new DateOnly(2022, 6, 15), "Active", updatedAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Update_ShouldThrowArgumentException_WhenStatusIsEmpty()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 5, 12, 10, 0, 0, DateTimeKind.Utc);
        var patient = Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            patient.Update("Thor", new DateOnly(2022, 6, 15), "   ", updatedAt));
        Assert.StartsWith("Status is required", exception.Message);
        Assert.Equal("status", exception.ParamName);
    }

    [Fact]
    public void Update_ShouldThrowArgumentException_WhenUpdatedAtIsDefault()
    {
        // Arrange
        var createdAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = default(DateTime);
        var patient = Patient.Create(1, PatientType.Pet, "Thor", new DateOnly(2021, 5, 10), "Active", createdAt);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            patient.Update("Thor", new DateOnly(2022, 6, 15), "Active", updatedAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("updatedAt", exception.ParamName);
    }
}
