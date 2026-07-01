using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Repositories;

public class UserClinicAccessRepositoryTests
{
    [Fact]
    public async Task HasActiveAccessAsync_ActiveAccess_ShouldReturnTrue()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var (user, clinic) = await CreateUserAndClinicAsync(context);
        var access = UserClinicAccess.Create(user.Id, clinic.Id, DateTime.UtcNow);
        context.UserClinicAccesses.Add(access);
        await context.SaveChangesAsync();
        var repository = new UserClinicAccessRepository(context);

        var hasAccess = await repository.HasActiveAccessAsync(user.Id, clinic.Id);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasActiveAccessAsync_MissingAccess_ShouldReturnFalse()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var (user, clinic) = await CreateUserAndClinicAsync(context);
        var repository = new UserClinicAccessRepository(context);

        var hasAccess = await repository.HasActiveAccessAsync(user.Id, clinic.Id);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasActiveAccessAsync_InactiveAccess_ShouldReturnFalse()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var (user, clinic) = await CreateUserAndClinicAsync(context);
        var access = UserClinicAccess.Create(user.Id, clinic.Id, DateTime.UtcNow);
        access.Inactivate(DateTime.UtcNow.AddMinutes(1));
        context.UserClinicAccesses.Add(access);
        await context.SaveChangesAsync();
        var repository = new UserClinicAccessRepository(context);

        var hasAccess = await repository.HasActiveAccessAsync(user.Id, clinic.Id);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task UserClinicAccess_ShouldPersistAndBlockDuplicateUserClinicPair()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var (user, clinic) = await CreateUserAndClinicAsync(context);
        context.UserClinicAccesses.Add(UserClinicAccess.Create(user.Id, clinic.Id, DateTime.UtcNow));
        await context.SaveChangesAsync();

        context.UserClinicAccesses.Add(UserClinicAccess.Create(user.Id, clinic.Id, DateTime.UtcNow.AddMinutes(1)));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }


    private static async Task<(User User, Clinic Clinic)> CreateUserAndClinicAsync(AppDbContext context)
    {
        var user = User.Create("User", "user@example.com", "hash123456");
        var organization = Organization.Create("Org", DateTime.UtcNow);
        context.Users.Add(user);
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var clinic = Clinic.Create(organization.Id, "Clinic", DateTime.UtcNow);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        return (user, clinic);
    }
}
