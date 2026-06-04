namespace Togo.Application.Auditing;

public interface IClinicalAuditLogWriter
{
    Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken);
}
