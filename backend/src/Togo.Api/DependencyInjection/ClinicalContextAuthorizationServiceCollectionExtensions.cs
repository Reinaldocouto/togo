using Togo.Application.Security;
using Togo.Infrastructure.Repositories;

namespace Togo.Api.DependencyInjection;

public static class ClinicalContextAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddClinicalContextAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IUserClinicAccessRepository, UserClinicAccessRepository>();
        services.AddScoped<IClinicalContextAuthorizationService, ClinicalContextAuthorizationService>();

        return services;
    }
}
