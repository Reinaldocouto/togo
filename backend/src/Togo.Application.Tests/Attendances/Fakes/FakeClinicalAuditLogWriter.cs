using Togo.Application.Auditing;

namespace Togo.Application.Tests.Attendances.Fakes;

internal sealed class FakeClinicalAuditLogWriter : IClinicalAuditLogWriter
{
    public List<ClinicalAuditEvent> Events { get; } = [];
    public int WriteCallsCount => Events.Count;
    public bool ThrowOnWrite { get; set; }

    public Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ThrowOnWrite)
        {
            throw new InvalidOperationException("Clinical audit log write failed for the test.");
        }

        Events.Add(auditEvent);
        return Task.CompletedTask;
    }
}
