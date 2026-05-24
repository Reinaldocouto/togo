namespace Togo.Application.MedicalRecords.Contracts;

public record MedicalRecordListItemResponse(
    long Id,
    long PatientId,
    DateTime UpdatedAt,
    bool HasGeneralNotes,
    bool HasFlags);
