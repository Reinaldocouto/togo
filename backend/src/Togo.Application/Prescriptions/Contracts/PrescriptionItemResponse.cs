namespace Togo.Application.Prescriptions.Contracts;

public sealed record PrescriptionItemResponse(
    long Id,
    long? ProductId,
    decimal Quantity,
    string Unit,
    string Dosage,
    int? DurationDays);
