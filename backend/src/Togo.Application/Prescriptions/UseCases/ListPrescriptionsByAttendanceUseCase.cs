using Togo.Application.Attendances.Repositories;
using Togo.Application.Prescriptions.Contracts;
using Togo.Application.Prescriptions.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.Prescriptions.UseCases;

public class ListPrescriptionsByAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public ListPrescriptionsByAttendanceUseCase(IAttendanceRepository attendanceRepository, IPrescriptionRepository prescriptionRepository)
    {
        _attendanceRepository = attendanceRepository;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<ApplicationResult<IReadOnlyList<PrescriptionListItemResponse>>> ExecuteAsync(long attendanceId, CancellationToken cancellationToken)
    {
        if (attendanceId <= 0)
        {
            return ApplicationResult<IReadOnlyList<PrescriptionListItemResponse>>.ValidationError("Attendance id is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendance is null)
        {
            return ApplicationResult<IReadOnlyList<PrescriptionListItemResponse>>.NotFound("Attendance not found.");
        }

        var prescriptions = await _prescriptionRepository.ListByAttendanceIdAsync(attendanceId, cancellationToken);
        var response = prescriptions
            .Select(prescription => new PrescriptionListItemResponse(prescription.Id, prescription.AttendanceId, prescription.IssuedAt, prescription.ItemCount))
            .ToList();

        return ApplicationResult<IReadOnlyList<PrescriptionListItemResponse>>.Success(response);
    }
}
