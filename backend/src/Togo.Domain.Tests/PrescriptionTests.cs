using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class PrescriptionTests
{
    [Fact] public void Create_ShouldCreateValidPrescriptionWithClinicId() { var p = Prescription.Create(2, 1, DateTime.UtcNow, " notes "); Assert.Equal(2, p.ClinicId); Assert.Equal(1, p.AttendanceId); Assert.Equal("notes", p.Notes); }
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidClinicId(long id) => Assert.Throws<ArgumentOutOfRangeException>(() => Prescription.Create(id, 1, DateTime.UtcNow, null));
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidAttendanceId(long id) => Assert.Throws<ArgumentOutOfRangeException>(() => Prescription.Create(2, id, DateTime.UtcNow, null));
    [Fact] public void Create_ShouldRejectDefaultIssuedAt() => Assert.Throws<ArgumentException>(() => Prescription.Create(2, 1, default, null));
    [Theory][InlineData(null)][InlineData("")][InlineData("   ")] public void Create_ShouldNormalizeEmptyNotesToNull(string? notes) => Assert.Null(Prescription.Create(2, 1, DateTime.UtcNow, notes).Notes);
    [Fact] public void UpdateNotes_ShouldNormalizeValuesAndPreserveClinicId() { var p = Prescription.Create(2, 1, DateTime.UtcNow, "a"); p.UpdateNotes(" b "); Assert.Equal(2, p.ClinicId); Assert.Equal("b", p.Notes); p.UpdateNotes(" "); Assert.Equal(2, p.ClinicId); Assert.Null(p.Notes); }
}
