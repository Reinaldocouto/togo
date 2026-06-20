using Microsoft.Extensions.Logging;
using Togo.Application.Attendances.Repositories;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.ClinicalEvolutions.UseCases;

public class CreateClinicalEvolutionUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IClinicalEvolutionRepository _clinicalEvolutionRepository;
    private readonly ILogger<CreateClinicalEvolutionUseCase> _logger;

    public CreateClinicalEvolutionUseCase(
        IAttendanceRepository attendanceRepository,
        IClinicalEvolutionRepository clinicalEvolutionRepository,
        ILogger<CreateClinicalEvolutionUseCase> logger)
    {
        _attendanceRepository = attendanceRepository;
        _clinicalEvolutionRepository = clinicalEvolutionRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<ClinicalEvolutionResponse>> ExecuteAsync(
        long attendanceId,
        CreateClinicalEvolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (attendanceId <= 0)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Attendance id is invalid.");
        }

        if (request.AttendanceId <= 0)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Request attendance id is invalid.");
        }

        if (request.AttendanceId != attendanceId)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Route attendance id must match request attendance id.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError("Clinical evolution type is invalid.");
        }

        var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendance is null)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.NotFound("Attendance not found.");
        }

        if (attendance.Status != AttendanceStatus.Open)
        {
            return ApplicationResult<ClinicalEvolutionResponse>.Conflict("Clinical evolution can only be created for an open attendance.");
        }

        try
        {
            var clinicalEvolution = ClinicalEvolution.Create(
                request.AttendanceId,
                request.RegisteredAt,
                request.Type,
                request.Text);

            await _clinicalEvolutionRepository.AddAsync(clinicalEvolution, cancellationToken);

            return ApplicationResult<ClinicalEvolutionResponse>.Success(ToResponse(clinicalEvolution));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Clinical evolution creation failed due to validation error. AttendanceId: {AttendanceId}", attendanceId);
            return ApplicationResult<ClinicalEvolutionResponse>.ValidationError(ex.Message);
        }
    }

    private static ClinicalEvolutionResponse ToResponse(ClinicalEvolution clinicalEvolution) =>
        new(
            clinicalEvolution.Id,
            clinicalEvolution.AttendanceId,
            clinicalEvolution.RegisteredAt,
            clinicalEvolution.Type,
            clinicalEvolution.Text);
}
