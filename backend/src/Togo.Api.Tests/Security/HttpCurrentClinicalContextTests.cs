using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Togo.Api.DependencyInjection;
using Togo.Api.Security;
using Togo.Application.Security;

namespace Togo.Api.Tests.Security;

public class HttpCurrentClinicalContextTests
{
    [Fact]
    public void ClinicId_ShouldBeNull_AndHasClinicFalse_WhenHeaderIsAbsent()
    {
        var context = CreateContext();

        Assert.Null(context.ClinicId);
        Assert.False(context.HasClinic);
    }

    [Fact]
    public void ClinicId_ShouldResolvePositiveLong_FromClinicHeader()
    {
        var context = CreateContext("42");

        Assert.Equal(42, context.ClinicId);
        Assert.True(context.HasClinic);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("")]
    public void ClinicId_ShouldFailPredictably_WhenClinicHeaderIsInvalid(string value)
    {
        var context = CreateContext(value);

        Assert.Throws<InvalidClinicalContextException>(() => context.ClinicId);
    }

    [Fact]
    public void GetRequiredClinicId_ShouldFail_WhenHeaderIsAbsent()
    {
        var context = CreateContext();

        Assert.Throws<MissingClinicalContextException>(() => context.GetRequiredClinicId());
    }

    [Fact]
    public void Service_ShouldBeRegisteredAsScoped()
    {
        var services = new ServiceCollection();

        services.AddCurrentClinicalContext();

        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(ICurrentClinicalContext));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(HttpCurrentClinicalContext), descriptor.ImplementationType);
    }

    [Fact]
    public void ContextResolution_ShouldNotRequireUserClinicAccessValidation()
    {
        var context = CreateContext("99");

        Assert.Equal(99, context.GetRequiredClinicId());
    }

    private static HttpCurrentClinicalContext CreateContext(string? clinicIdHeader = null)
    {
        var httpContext = new DefaultHttpContext();

        if (clinicIdHeader is not null)
        {
            httpContext.Request.Headers[HttpCurrentClinicalContext.ClinicIdHeaderName] = clinicIdHeader;
        }

        return new HttpCurrentClinicalContext(new HttpContextAccessor
        {
            HttpContext = httpContext
        });
    }
}
