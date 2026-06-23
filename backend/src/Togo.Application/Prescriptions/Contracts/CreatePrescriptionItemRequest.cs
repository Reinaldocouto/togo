namespace Togo.Application.Prescriptions.Contracts;

public sealed record CreatePrescriptionItemRequest(
    long? ProductId,
    decimal Quantity,
    string Unit,
    string Dosage,
    int? DurationDays);
