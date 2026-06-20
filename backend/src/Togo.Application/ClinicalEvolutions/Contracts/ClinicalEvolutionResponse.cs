using Togo.Domain.Enums;

namespace Togo.Application.ClinicalEvolutions.Contracts;

public sealed record ClinicalEvolutionResponse(
    long Id,
    long AttendanceId,
    DateTime RegisteredAt,
    EvolutionType Type,
    string Text);
