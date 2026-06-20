using Togo.Application.Attendances.Repositories;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Application.Tutors;

namespace Togo.Application.ClinicalEvolutions.UseCases;

public class ListClinicalEvolutionsByAttendanceUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IClinicalEvolutionRepository _clinicalEvolutionRepository;

    public ListClinicalEvolutionsByAttendanceUseCase(
        IAttendanceRepository attendanceRepository,
        IClinicalEvolutionRepository clinicalEvolutionRepository)
    {
        _attendanceRepository = attendanceRepository;
        _clinicalEvolutionRepository = clinicalEvolutionRepository;
    }

    public async Task<ApplicationResult<IReadOnlyList<ClinicalEvolutionListItemResponse>>> ExecuteAsync(
        long attendanceId,
        CancellationToken cancellationToken)
    {
        if (attendanceId <= 0)
        {
            return ApplicationResult<IReadOnlyList<ClinicalEvolutionListItemResponse>>.ValidationError("Attendance id is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendance is null)
        {
            return ApplicationResult<IReadOnlyList<ClinicalEvolutionListItemResponse>>.NotFound("Attendance not found.");
        }

        var clinicalEvolutions = await _clinicalEvolutionRepository.ListByAttendanceIdAsync(attendanceId, cancellationToken);
        var response = clinicalEvolutions
            .Select(evolution => new ClinicalEvolutionListItemResponse(
                evolution.Id,
                evolution.AttendanceId,
                evolution.RegisteredAt,
                evolution.Type))
            .ToList();

        return ApplicationResult<IReadOnlyList<ClinicalEvolutionListItemResponse>>.Success(response);
    }
}
