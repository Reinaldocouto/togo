namespace Togo.Domain.Entities;

public class ClinicalAuditLog
{
    private ClinicalAuditLog() { }

    private ClinicalAuditLog(
        string entityName,
        string entityId,
        string action,
        Guid userId,
        string? userProfile,
        DateTime occurredAt,
        string? metadataJson)
    {
        ValidateRequired(entityName, nameof(entityName));
        ValidateRequired(entityId, nameof(entityId));
        ValidateRequired(action, nameof(action));

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required", nameof(userId));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException("OccurredAt is required", nameof(occurredAt));
        }

        EntityName = entityName.Trim();
        EntityId = entityId.Trim();
        Action = action.Trim();
        UserId = userId;
        UserProfile = string.IsNullOrWhiteSpace(userProfile) ? null : userProfile.Trim();
        OccurredAt = occurredAt.Kind == DateTimeKind.Utc
            ? occurredAt
            : occurredAt.ToUniversalTime();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
    }

    public long Id { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string? UserProfile { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public static ClinicalAuditLog Create(
        string entityName,
        string entityId,
        string action,
        Guid userId,
        string? userProfile,
        DateTime occurredAt,
        string? metadataJson) =>
        new(entityName, entityId, action, userId, userProfile, occurredAt, metadataJson);

    private static void ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required", paramName);
        }
    }
}
