using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Domain.Tests;

public class ClinicalEvolutionTests
{
    private static readonly Guid CreatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UpdatedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime RegisteredAt = new(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CreatedAt = new(2026, 6, 20, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAt = new(2026, 6, 20, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldCreateClinicalEvolution_WhenDataIsValid()
    {
        var evolution = ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "  Patient stable  ", CreatedByUserId, CreatedAt);

        Assert.Equal(10, evolution.AttendanceId);
        Assert.Equal(RegisteredAt, evolution.RegisteredAt);
        Assert.Equal(EvolutionType.ClinicalNote, evolution.Type);
        Assert.Equal("Patient stable", evolution.Text);
        Assert.Equal(CreatedByUserId, evolution.CreatedByUserId);
        Assert.Equal(CreatedAt, evolution.CreatedAt);
        Assert.Equal(CreatedByUserId, evolution.UpdatedByUserId);
        Assert.Equal(CreatedAt, evolution.UpdatedAt);
        Assert.NotEqual(RegisteredAt, evolution.CreatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenAttendanceIdIsInvalid(long attendanceId)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ClinicalEvolution.Create(attendanceId, RegisteredAt, EvolutionType.ClinicalNote, "note", CreatedByUserId, CreatedAt));

        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("attendanceId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenRegisteredAtIsDefault()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, default, EvolutionType.ClinicalNote, "note", CreatedByUserId, CreatedAt));

        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("registeredAt", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenTextIsEmpty(string text)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, text, CreatedByUserId, CreatedAt));

        Assert.StartsWith("Text is required", exception.Message);
        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedByUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "note", Guid.Empty, CreatedAt));

        Assert.StartsWith("User id is required", exception.Message);
        Assert.Equal("createdByUserId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedAtIsDefault()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "note", CreatedByUserId, default));

        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void UpdateText_ShouldValidateAndTrimText()
    {
        var evolution = ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "note", CreatedByUserId, CreatedAt);

        evolution.UpdateText("  updated note  ", UpdatedByUserId, UpdatedAt);

        Assert.Equal("updated note", evolution.Text);
        Assert.Equal(UpdatedByUserId, evolution.UpdatedByUserId);
        Assert.Equal(UpdatedAt, evolution.UpdatedAt);
        Assert.Throws<ArgumentException>(() => evolution.UpdateText("   ", UpdatedByUserId, UpdatedAt));
        Assert.Throws<ArgumentException>(() => evolution.UpdateText("updated", Guid.Empty, UpdatedAt));
        Assert.Throws<ArgumentException>(() => evolution.UpdateText("updated", UpdatedByUserId, default));
    }
}
