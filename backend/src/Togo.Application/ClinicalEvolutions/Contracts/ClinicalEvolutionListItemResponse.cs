using Togo.Domain.Enums;

namespace Togo.Application.ClinicalEvolutions.Contracts;

public sealed record ClinicalEvolutionListItemResponse(
    long Id,
    long AttendanceId,
    DateTime RegisteredAt,
    EvolutionType Type);
