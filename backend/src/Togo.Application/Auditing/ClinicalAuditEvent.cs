namespace Togo.Application.Auditing;

public sealed record ClinicalAuditEvent(
    string EntityName,
    string EntityId,
    string Action,
    Guid UserId,
    string? UserProfile,
    DateTime OccurredAt,
    string? MetadataJson);
