using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Attendances.UseCases;

public class ListAttendancesUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<ListAttendancesUseCase> _logger;

    public ListAttendancesUseCase(
        IAttendanceRepository attendanceRepository,
        ILogger<ListAttendancesUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<IReadOnlyList<AttendanceListItemResponse>>> ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing attendances");

        var attendances = await _attendanceRepository.ListAsync(cancellationToken);
        var responses = attendances
            .Select(ToListItemResponse)
            .ToList();

        _logger.LogInformation("Attendances listed successfully. Count: {Count}", responses.Count);

        return ApplicationResult<IReadOnlyList<AttendanceListItemResponse>>.Success(responses);
    }

    private static AttendanceListItemResponse ToListItemResponse(Attendance attendance) =>
        new(
            attendance.Id,
            attendance.PatientId,
            attendance.AttendanceNumber,
            attendance.OpenedAt,
            attendance.ClosedAt,
            attendance.Status,
            attendance.Type);
}
