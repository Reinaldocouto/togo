namespace Togo.Application.MedicalRecords.Contracts;

public record MedicalRecordResponse(
    long Id,
    long PatientId,
    string? GeneralNotes,
    string? FlagsJson,
    DateTime UpdatedAt);
