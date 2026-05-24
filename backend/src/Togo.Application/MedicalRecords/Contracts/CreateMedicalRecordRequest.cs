namespace Togo.Application.MedicalRecords.Contracts;

public record CreateMedicalRecordRequest(
    string? GeneralNotes,
    string? FlagsJson);
