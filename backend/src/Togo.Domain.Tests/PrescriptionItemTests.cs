using Togo.Domain.Entities;

namespace Togo.Domain.Tests;

public class PrescriptionItemTests
{
    [Fact] public void Create_ShouldCreateWithProductId() { var i = PrescriptionItem.Create(1, 2, 1, " ml ", " bid ", 3); Assert.Equal(2, i.ProductId); Assert.Equal("ml", i.Unit); Assert.Equal("bid", i.Dosage); }
    [Fact] public void Create_ShouldCreateWithoutProductId() => Assert.Null(PrescriptionItem.Create(1, null, 1, "ml", "bid", null).ProductId);
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidPrescriptionId(long id) => Assert.Throws<ArgumentOutOfRangeException>(() => PrescriptionItem.Create(id, null, 1, "ml", "bid", null));
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidProductId(long id) => Assert.Throws<ArgumentOutOfRangeException>(() => PrescriptionItem.Create(1, id, 1, "ml", "bid", null));
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidQuantity(decimal q) => Assert.Throws<ArgumentOutOfRangeException>(() => PrescriptionItem.Create(1, null, q, "ml", "bid", null));
    [Theory][InlineData("")][InlineData(" ")] public void Create_ShouldRejectEmptyUnit(string unit) => Assert.Throws<ArgumentException>(() => PrescriptionItem.Create(1, null, 1, unit, "bid", null));
    [Theory][InlineData("")][InlineData(" ")] public void Create_ShouldRejectEmptyDosage(string dosage) => Assert.Throws<ArgumentException>(() => PrescriptionItem.Create(1, null, 1, "ml", dosage, null));
    [Theory][InlineData(0)][InlineData(-1)] public void Create_ShouldRejectInvalidDuration(int days) => Assert.Throws<ArgumentOutOfRangeException>(() => PrescriptionItem.Create(1, null, 1, "ml", "bid", days));
    [Fact] public void Update_ShouldValidateAndTrim() { var i = PrescriptionItem.Create(1, null, 1, "ml", "bid", null); i.Update(2, " tab ", " sid ", 5); Assert.Equal("tab", i.Unit); Assert.Equal("sid", i.Dosage); Assert.Throws<ArgumentException>(() => i.Update(2, " ", "sid", 5)); }
}
