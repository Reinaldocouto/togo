using Togo.Application.Auditing;

namespace Togo.Application.Tests.Prescriptions.Fakes;

internal sealed class FakeClinicalAuditLogWriter : IClinicalAuditLogWriter
{
    public List<ClinicalAuditEvent> Events { get; } = [];
    public int WriteCallsCount { get; private set; }

    public Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WriteCallsCount++;
        Events.Add(auditEvent);
        return Task.CompletedTask;
    }
}
