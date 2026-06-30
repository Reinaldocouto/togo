using System.Text.Json;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Auditing;
using Togo.Application.Prescriptions.Contracts;
using Togo.Application.Prescriptions.Repositories;
using Togo.Application.Security;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Prescriptions.UseCases;

public class CreatePrescriptionUseCase
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicalAuditLogWriter _clinicalAuditLogWriter;

    public CreatePrescriptionUseCase(
        IAttendanceRepository attendanceRepository,
        IPrescriptionRepository prescriptionRepository,
        ICurrentUserService currentUserService,
        IClinicalAuditLogWriter clinicalAuditLogWriter)
    {
        _attendanceRepository = attendanceRepository;
        _prescriptionRepository = prescriptionRepository;
        _currentUserService = currentUserService;
        _clinicalAuditLogWriter = clinicalAuditLogWriter;
    }

    public async Task<ApplicationResult<PrescriptionResponse>> ExecuteAsync(long attendanceId, CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var validationError = Validate(attendanceId, request);
        if (validationError is not null)
        {
            return ApplicationResult<PrescriptionResponse>.ValidationError(validationError);
        }

        var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendance is null)
        {
            return ApplicationResult<PrescriptionResponse>.NotFound("Attendance not found.");
        }

        if (attendance.Status != AttendanceStatus.Open)
        {
            return ApplicationResult<PrescriptionResponse>.Conflict("Prescription can only be created for open attendances.");
        }

        var currentUser = _currentUserService.GetCurrentUser();
        var prescription = Prescription.Create(attendance.ClinicId, request.AttendanceId, request.IssuedAt, request.Notes);
        var itemDrafts = request.Items
            .Select(item => new PrescriptionItemDraft(item.ProductId, item.Quantity, item.Unit, item.Dosage, item.DurationDays))
            .ToList();

        await _prescriptionRepository.AddAsync(prescription, itemDrafts, cancellationToken);
        await WriteCreatedAuditLogAsync(prescription, currentUser, cancellationToken);

        var responseItems = itemDrafts
            .Select(item => new PrescriptionItemResponse(0, item.ProductId, item.Quantity, item.Unit.Trim(), item.Dosage.Trim(), item.DurationDays))
            .ToList();

        return ApplicationResult<PrescriptionResponse>.Success(new PrescriptionResponse(
            prescription.Id,
            prescription.ClinicId,
            prescription.AttendanceId,
            prescription.IssuedAt,
            prescription.Notes,
            responseItems));
    }

    private async Task WriteCreatedAuditLogAsync(Prescription prescription, CurrentUserInfo currentUser, CancellationToken cancellationToken)
    {
        var auditEvent = new ClinicalAuditEvent(
            EntityName: nameof(Prescription),
            EntityId: prescription.Id.ToString(),
            Action: PrescriptionAuditActions.Created,
            UserId: currentUser.UserId,
            UserProfile: currentUser.Profile,
            OccurredAt: DateTime.UtcNow,
            MetadataJson: CreateMetadataJson(prescription.ClinicId, prescription.AttendanceId));

        await _clinicalAuditLogWriter.WriteAsync(auditEvent, cancellationToken);
    }

    private static string CreateMetadataJson(long clinicId, long attendanceId) =>
        JsonSerializer.Serialize(new { ClinicId = clinicId, AttendanceId = attendanceId });

    private static string? Validate(long attendanceId, CreatePrescriptionRequest request)
    {
        if (attendanceId <= 0) return "Attendance id is invalid.";
        if (request.AttendanceId <= 0) return "Request attendance id is invalid.";
        if (attendanceId != request.AttendanceId) return "Route attendance id must match request attendance id.";
        if (request.IssuedAt == default) return "IssuedAt is required.";
        if (request.Items is null || request.Items.Count == 0) return "At least one prescription item is required.";

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0) return "Item quantity must be greater than zero.";
            if (string.IsNullOrWhiteSpace(item.Unit)) return "Item unit is required.";
            if (string.IsNullOrWhiteSpace(item.Dosage)) return "Item dosage is required.";
            if (item.DurationDays.HasValue && item.DurationDays.Value <= 0) return "Item duration must be greater than zero.";
            if (item.ProductId.HasValue && item.ProductId.Value <= 0) return "Item product id must be greater than zero.";
        }

        return null;
    }
}
