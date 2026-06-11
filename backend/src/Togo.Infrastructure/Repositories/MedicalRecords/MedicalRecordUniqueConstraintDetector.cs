using Microsoft.EntityFrameworkCore;

namespace Togo.Infrastructure.Repositories.MedicalRecords;

internal static class MedicalRecordUniqueConstraintDetector
{
    internal const string MedicalRecordsPatientIdIndexName = "IX_MedicalRecords_PatientId";
    private const string MedicalRecordsTableName = "MedicalRecords";
    private const string PatientIdColumnName = "PatientId";
    private const int SqliteConstraintErrorCode = 19;
    private const int SqliteConstraintUniqueExtendedErrorCode = 2067;
    private const int MySqlDuplicateEntryErrorCode = 1062;

    public static bool IsMedicalRecordPatientIdUniqueConstraintViolation(DbUpdateException exception)
    {
        var databaseException = exception.GetBaseException();
        var message = databaseException.Message;

        return IsSqliteMedicalRecordPatientIdUniqueConstraintViolation(databaseException, message)
            || IsMySqlMedicalRecordPatientIdUniqueConstraintViolation(databaseException, message);
    }

    private static bool IsSqliteMedicalRecordPatientIdUniqueConstraintViolation(Exception exception, string message)
    {
        if (!string.Equals(exception.GetType().FullName, "Microsoft.Data.Sqlite.SqliteException", StringComparison.Ordinal))
        {
            return false;
        }

        var sqliteErrorCode = ReadIntProperty(exception, "SqliteErrorCode");
        var sqliteExtendedErrorCode = ReadIntProperty(exception, "SqliteExtendedErrorCode");

        if (sqliteErrorCode != SqliteConstraintErrorCode || sqliteExtendedErrorCode != SqliteConstraintUniqueExtendedErrorCode)
        {
            return false;
        }

        return message.Contains($"{MedicalRecordsTableName}.{PatientIdColumnName}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMySqlMedicalRecordPatientIdUniqueConstraintViolation(Exception exception, string message)
    {
        if (!string.Equals(exception.GetType().FullName, "MySqlConnector.MySqlException", StringComparison.Ordinal))
        {
            return false;
        }

        var errorCode = ReadIntProperty(exception, "Number");
        if (errorCode != MySqlDuplicateEntryErrorCode)
        {
            return false;
        }

        return message.Contains(MedicalRecordsPatientIdIndexName, StringComparison.OrdinalIgnoreCase)
            || (message.Contains(MedicalRecordsTableName, StringComparison.OrdinalIgnoreCase)
                && message.Contains(PatientIdColumnName, StringComparison.OrdinalIgnoreCase));
    }

    private static int? ReadIntProperty(Exception exception, string propertyName)
    {
        var value = exception.GetType().GetProperty(propertyName)?.GetValue(exception);
        return value switch
        {
            int intValue => intValue,
            uint uintValue when uintValue <= int.MaxValue => (int)uintValue,
            _ => null
        };
    }
}
