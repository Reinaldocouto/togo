using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class PrescriptionTests
{
    [Fact] public void Create_ShouldCreateValidPrescription() { var p = Prescription.Create(1, DateTime.UtcNow, " notes "); Assert.Equal(1, p.AttendanceId); Assert.Equal("notes", p.Notes); }
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidAttendanceId(long id) => Assert.Throws<ArgumentOutOfRangeException>(() => Prescription.Create(id, DateTime.UtcNow, null));
    [Fact] public void Create_ShouldRejectDefaultIssuedAt() => Assert.Throws<ArgumentException>(() => Prescription.Create(1, default, null));
    [Theory][InlineData(null)][InlineData("")][InlineData("   ")] public void Create_ShouldNormalizeEmptyNotesToNull(string? notes) => Assert.Null(Prescription.Create(1, DateTime.UtcNow, notes).Notes);
    [Fact] public void UpdateNotes_ShouldNormalizeValues() { var p = Prescription.Create(1, DateTime.UtcNow, "a"); p.UpdateNotes(" b "); Assert.Equal("b", p.Notes); p.UpdateNotes(" "); Assert.Null(p.Notes); }
}
