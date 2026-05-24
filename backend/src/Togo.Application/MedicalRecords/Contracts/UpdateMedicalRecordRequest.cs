namespace Togo.Application.MedicalRecords.Contracts;

public record UpdateMedicalRecordRequest(
    string? GeneralNotes,
    string? FlagsJson);
