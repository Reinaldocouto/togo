using Togo.Domain.Enums;

namespace Togo.Application.ClinicalEvolutions.Contracts;

public sealed record CreateClinicalEvolutionRequest(
    long AttendanceId,
    DateTime RegisteredAt,
    EvolutionType Type,
    string Text);
