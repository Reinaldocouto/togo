using Togo.Application.Security;

namespace Togo.Application.Tests.Security;

public class CurrentClinicalContextTests
{
    [Fact]
    public void GetRequiredClinicId_ShouldReturnClinicId_WhenPresent()
    {
        var context = new TestCurrentClinicalContext(123);

        var clinicId = context.GetRequiredClinicId();

        Assert.Equal(123, clinicId);
    }

    [Fact]
    public void GetRequiredClinicId_ShouldFail_WhenAbsent()
    {
        var context = new TestCurrentClinicalContext(null);

        Assert.Throws<MissingClinicalContextException>(() => context.GetRequiredClinicId());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(1L, true)]
    public void HasClinic_ShouldReflectClinicIdPresence(long? clinicId, bool expectedHasClinic)
    {
        var context = new TestCurrentClinicalContext(clinicId);

        Assert.Equal(expectedHasClinic, context.HasClinic);
    }

    private sealed class TestCurrentClinicalContext : ICurrentClinicalContext
    {
        public TestCurrentClinicalContext(long? clinicId)
        {
            ClinicId = clinicId;
        }

        public long? ClinicId { get; }

        public bool HasClinic => ClinicId.HasValue;

        public long GetRequiredClinicId()
        {
            return ClinicId ?? throw new MissingClinicalContextException();
        }
    }
}
