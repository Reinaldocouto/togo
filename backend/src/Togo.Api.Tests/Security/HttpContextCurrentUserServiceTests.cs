using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Togo.Api.Security;
using Togo.Application.Security;
using Togo.Domain.Security;
using Xunit;

namespace Togo.Api.Tests.Security;

public class HttpContextCurrentUserServiceTests
{
    [Fact]
    public void GetCurrentUser_ShouldResolveUserIdFromNameIdentifier()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())));

        var currentUser = service.GetCurrentUser();

        Assert.Equal(userId, currentUser.UserId);
        Assert.True(currentUser.IsAuthenticated);
    }

    [Fact]
    public void GetCurrentUser_ShouldPreferNameIdentifierOverSubject()
    {
        var nameIdentifier = Guid.NewGuid();
        var subject = Guid.NewGuid();
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier.ToString()),
            new Claim("sub", subject.ToString())));

        var currentUser = service.GetCurrentUser();

        Assert.Equal(nameIdentifier, currentUser.UserId);
    }

    [Fact]
    public void GetCurrentUser_ShouldFailWhenPreferredNameIdentifierIsInvalid()
    {
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid"),
            new Claim("sub", Guid.NewGuid().ToString())));

        Assert.Throws<CurrentUserResolutionException>(() => service.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_ShouldResolveUserIdFromSubjectAsFallback()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim("sub", userId.ToString())));

        var currentUser = service.GetCurrentUser();

        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void GetCurrentUser_ShouldResolveProfileWhenAvailable()
    {
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(TogoClaimTypes.Profile, "Veterinarian")));

        var currentUser = service.GetCurrentUser();

        Assert.Equal("Veterinarian", currentUser.Profile);
    }

    [Fact]
    public void GetCurrentUser_ShouldNotRequireProfileNameOrEmail()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())));

        var currentUser = service.GetCurrentUser();

        Assert.Equal(userId, currentUser.UserId);
        Assert.Null(currentUser.Profile);
    }

    [Fact]
    public void GetCurrentUser_ShouldFailWhenHttpContextIsMissing()
    {
        var service = new HttpContextCurrentUserService(new HttpContextAccessor());

        Assert.Throws<CurrentUserResolutionException>(() => service.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_ShouldFailWhenIdentityIsNotAuthenticated()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        var service = CreateService(principal);

        Assert.Throws<CurrentUserResolutionException>(() => service.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_ShouldFailWhenUserIdIsMissing()
    {
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(TogoClaimTypes.Profile, "Veterinarian")));

        Assert.Throws<CurrentUserResolutionException>(() => service.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_ShouldFailWhenUserIdIsNotAValidGuid()
    {
        var service = CreateService(CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")));

        Assert.Throws<CurrentUserResolutionException>(() => service.GetCurrentUser());
    }

    private static HttpContextCurrentUserService CreateService(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        return new HttpContextCurrentUserService(new HttpContextAccessor
        {
            HttpContext = httpContext
        });
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }
}
