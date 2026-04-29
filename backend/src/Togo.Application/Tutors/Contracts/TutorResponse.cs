namespace Togo.Application.Tutors.Contracts;

public record TutorResponse(long Id, string Name, string? Document, string? Email, string? Phone, DateTime CreatedAt, DateTime? UpdatedAt);
