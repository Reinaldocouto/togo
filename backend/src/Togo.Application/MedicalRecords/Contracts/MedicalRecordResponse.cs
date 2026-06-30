namespace Togo.Application.MedicalRecords.Contracts;

public record MedicalRecordResponse(
    long Id,
    long ClinicId,
    long PatientId,
    string? GeneralNotes,
    string? FlagsJson,
    DateTime UpdatedAt);
