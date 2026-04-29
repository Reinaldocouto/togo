using Togo.Application.Tutors.Contracts;
using Togo.Domain.Entities;

namespace Togo.Application.Tutors.UseCases;

internal static class TutorMappings
{
    public static TutorResponse ToResponse(Tutor tutor) =>
        new(tutor.Id, tutor.Name, tutor.Document, tutor.Email, tutor.Phone, tutor.CreatedAt, tutor.UpdatedAt);

    public static TutorListItemResponse ToListItemResponse(Tutor tutor) =>
        new(tutor.Id, tutor.Name, tutor.Document, tutor.Email, tutor.Phone);
}
