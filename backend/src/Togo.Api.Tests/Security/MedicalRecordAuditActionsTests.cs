using Togo.Application.Auditing;
using Xunit;

namespace Togo.Api.Tests.Security;

public class MedicalRecordAuditActionsTests
{
    [Fact]
    public void Constants_ShouldMatchExpectedValues()
    {
        Assert.Equal("MedicalRecord.Created", MedicalRecordAuditActions.Created);
        Assert.Equal("MedicalRecord.Updated", MedicalRecordAuditActions.Updated);
        Assert.Equal("MedicalRecord.Read", MedicalRecordAuditActions.Read);
        Assert.Equal("MedicalRecord.AccessDenied", MedicalRecordAuditActions.AccessDenied);
    }
}
