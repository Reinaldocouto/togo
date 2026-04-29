namespace Togo.Application.Tutors.Contracts;

public record CreateTutorRequest(string Name, string? Document, string? Email, string? Phone);
