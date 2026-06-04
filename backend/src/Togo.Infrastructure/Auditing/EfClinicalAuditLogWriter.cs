using Togo.Application.Auditing;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Auditing;

public sealed class EfClinicalAuditLogWriter : IClinicalAuditLogWriter
{
    private readonly AppDbContext _context;

    public EfClinicalAuditLogWriter(AppDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var auditLog = ClinicalAuditLog.Create(
            auditEvent.EntityName,
            auditEvent.EntityId,
            auditEvent.Action,
            auditEvent.UserId,
            auditEvent.UserProfile,
            auditEvent.OccurredAt,
            auditEvent.MetadataJson);

        await _context.ClinicalAuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
