using Togo.Api.Security;
using Togo.Application.Security;

namespace Togo.Api.DependencyInjection;

public static class ClinicalContextServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentClinicalContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentClinicalContext, HttpCurrentClinicalContext>();

        return services;
    }
}
