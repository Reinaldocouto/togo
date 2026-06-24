namespace Togo.Application.Prescriptions.Repositories;

public sealed record PrescriptionItemDraft(
    long? ProductId,
    decimal Quantity,
    string Unit,
    string Dosage,
    int? DurationDays);
