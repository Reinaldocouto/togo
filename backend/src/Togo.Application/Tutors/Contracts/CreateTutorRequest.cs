namespace Togo.Application.Tutors.Contracts;

public record CreateTutorRequest(long ClinicId, string Name, string? Document, string? Email, string? Phone);
