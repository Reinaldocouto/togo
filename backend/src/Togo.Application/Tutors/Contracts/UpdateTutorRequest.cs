namespace Togo.Application.Tutors.Contracts;

public record UpdateTutorRequest(string Name, string? Document, string? Email, string? Phone);
