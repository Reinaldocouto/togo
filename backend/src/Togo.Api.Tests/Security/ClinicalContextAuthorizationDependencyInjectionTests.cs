using Microsoft.Extensions.DependencyInjection;
using Togo.Api.DependencyInjection;
using Togo.Application.Security;
using Togo.Infrastructure.Repositories;

namespace Togo.Api.Tests.Security;

public class ClinicalContextAuthorizationDependencyInjectionTests
{
    [Fact]
    public void AddClinicalContextAuthorization_ShouldRegisterScopedServices()
    {
        var services = new ServiceCollection();

        services.AddClinicalContextAuthorization();

        AssertScoped<IUserClinicAccessRepository, UserClinicAccessRepository>(services);
        AssertScoped<IClinicalContextAuthorizationService, ClinicalContextAuthorizationService>(services);
    }

    private static void AssertScoped<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(TService));
        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
