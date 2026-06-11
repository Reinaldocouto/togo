namespace Togo.Application.MedicalRecords.Exceptions;

public sealed class MedicalRecordAlreadyExistsException : Exception
{
    public const string DefaultMessage = "Patient already has a medical record.";

    public MedicalRecordAlreadyExistsException(long patientId, Exception? innerException = null)
        : base(DefaultMessage, innerException)
    {
        PatientId = patientId;
    }

    public long PatientId { get; }
}
