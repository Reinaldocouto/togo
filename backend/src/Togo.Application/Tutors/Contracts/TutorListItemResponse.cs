namespace Togo.Application.Tutors.Contracts;

public record TutorListItemResponse(long Id, string Name, string? Document, string? Email, string? Phone);
