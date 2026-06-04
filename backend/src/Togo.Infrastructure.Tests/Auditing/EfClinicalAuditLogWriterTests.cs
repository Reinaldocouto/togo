using Microsoft.EntityFrameworkCore;
using Togo.Application.Auditing;
using Togo.Infrastructure.Auditing;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Auditing;

public sealed class EfClinicalAuditLogWriterTests
{
    [Fact]
    public async Task WriteAsync_ShouldPersistClinicalAuditLogWithoutClinicalPayload()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var writer = new EfClinicalAuditLogWriter(context);
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var occurredAt = new DateTime(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);
        var auditEvent = new ClinicalAuditEvent(
            EntityName: "MedicalRecord",
            EntityId: "123",
            Action: MedicalRecordAuditActions.Created,
            UserId: userId,
            UserProfile: "Veterinarian",
            OccurredAt: occurredAt,
            MetadataJson: "{\"PatientId\":456}");

        await writer.WriteAsync(auditEvent, CancellationToken.None);

        var persisted = await context.ClinicalAuditLogs.AsNoTracking().SingleAsync();
        Assert.Equal("MedicalRecord", persisted.EntityName);
        Assert.Equal("123", persisted.EntityId);
        Assert.Equal(MedicalRecordAuditActions.Created, persisted.Action);
        Assert.Equal(userId, persisted.UserId);
        Assert.Equal("Veterinarian", persisted.UserProfile);
        Assert.Equal(occurredAt, persisted.OccurredAt);
        Assert.Equal("{\"PatientId\":456}", persisted.MetadataJson);
        Assert.DoesNotContain("GeneralNotes", persisted.MetadataJson);
        Assert.DoesNotContain("FlagsJson", persisted.MetadataJson);
    }
}
