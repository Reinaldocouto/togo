using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Togo.Api.Controllers;
using Togo.Api.Security;
using Togo.Application.ClinicalEvolutions.Contracts;

namespace Togo.Api.Tests.Controllers;

public sealed class ClinicalEvolutionsControllerTests
{
    [Fact]
    public void Controller_ShouldRequireAuthorization()
    {
        Assert.Contains(typeof(ClinicalEvolutionsController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true), attribute => attribute is AuthorizeAttribute);
    }

    [Fact]
    public void Controller_ShouldExposeExpectedRoute()
    {
        var route = Assert.Single(typeof(ClinicalEvolutionsController).GetCustomAttributes(typeof(RouteAttribute), inherit: true).Cast<RouteAttribute>());
        Assert.Equal("api/attendances/{attendanceId:long}/clinical-evolutions", route.Template);
    }

    [Fact]
    public void ListByAttendance_ShouldUseReadPolicyAndHttpGet()
    {
        var method = typeof(ClinicalEvolutionsController).GetMethod(nameof(ClinicalEvolutionsController.ListByAttendance))!;

        Assert.Contains(method.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true), attribute => attribute is HttpGetAttribute);
        var authorize = Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(ClinicalEvolutionPolicies.Read, authorize.Policy);
    }

    [Fact]
    public void Create_ShouldUseCreatePolicyAndHttpPost()
    {
        var method = typeof(ClinicalEvolutionsController).GetMethod(nameof(ClinicalEvolutionsController.Create))!;

        Assert.Contains(method.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true), attribute => attribute is HttpPostAttribute);
        var authorize = Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(ClinicalEvolutionPolicies.Create, authorize.Policy);
    }

    [Fact]
    public void Controller_ShouldNotExposeUpdateOrDeleteEndpoints()
    {
        var publicInstanceMethods = typeof(ClinicalEvolutionsController)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(ClinicalEvolutionsController))
            .ToArray();

        Assert.DoesNotContain(publicInstanceMethods, method => method.GetCustomAttributes(typeof(HttpPutAttribute), inherit: true).Any());
        Assert.DoesNotContain(publicInstanceMethods, method => method.GetCustomAttributes(typeof(HttpPatchAttribute), inherit: true).Any());
        Assert.DoesNotContain(publicInstanceMethods, method => method.GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true).Any());
    }

    [Fact]
    public void ListItemResponse_ShouldNotExposeText()
    {
        Assert.DoesNotContain(typeof(ClinicalEvolutionListItemResponse).GetProperties(), property => property.Name == "Text");
    }

    [Fact]
    public void DetailResponse_ShouldExposeTextAndClinicIdForCreationResponse()
    {
        Assert.Contains(typeof(ClinicalEvolutionResponse).GetProperties(), property => property.Name == "Text");
        Assert.Contains(typeof(ClinicalEvolutionResponse).GetProperties(), property => property.Name == "ClinicId");
    }
}
