using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Domain.Tests;

public class ClinicalEvolutionTests
{
    private static readonly DateTime RegisteredAt = new(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldCreateClinicalEvolution_WhenDataIsValid()
    {
        var evolution = ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "  Patient stable  ");

        Assert.Equal(10, evolution.AttendanceId);
        Assert.Equal(RegisteredAt, evolution.RegisteredAt);
        Assert.Equal(EvolutionType.ClinicalNote, evolution.Type);
        Assert.Equal("Patient stable", evolution.Text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenAttendanceIdIsInvalid(long attendanceId)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ClinicalEvolution.Create(attendanceId, RegisteredAt, EvolutionType.ClinicalNote, "note"));

        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("attendanceId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenRegisteredAtIsDefault()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, default, EvolutionType.ClinicalNote, "note"));

        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("registeredAt", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenTextIsEmpty(string text)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, text));

        Assert.StartsWith("Text is required", exception.Message);
        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void UpdateText_ShouldValidateAndTrimText()
    {
        var evolution = ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "note");

        evolution.UpdateText("  updated note  ");

        Assert.Equal("updated note", evolution.Text);
        Assert.Throws<ArgumentException>(() => evolution.UpdateText("   "));
    }
}
