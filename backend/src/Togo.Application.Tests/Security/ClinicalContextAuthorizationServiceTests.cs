using Togo.Application.Security;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.Security;

public class ClinicalContextAuthorizationServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task EnsureCanAccessCurrentClinicAsync_ActiveAccess_ShouldAllow()
    {
        var repository = new FakeUserClinicAccessRepository((UserId, 10, true));
        var service = CreateService(repository, clinicId: 10, userId: UserId, isAuthenticated: true);

        await service.EnsureCanAccessCurrentClinicAsync();
    }

    [Fact]
    public async Task EnsureCanAccessCurrentClinicAsync_MissingAccess_ShouldDeny()
    {
        var service = CreateService(new FakeUserClinicAccessRepository(), clinicId: 10, userId: UserId, isAuthenticated: true);

        await Assert.ThrowsAsync<ClinicalContextAccessDeniedException>(() => service.EnsureCanAccessCurrentClinicAsync());
    }

    [Fact]
    public async Task EnsureCanAccessCurrentClinicAsync_InactiveAccess_ShouldDeny()
    {
        var repository = new FakeUserClinicAccessRepository((UserId, 10, false));
        var service = CreateService(repository, clinicId: 10, userId: UserId, isAuthenticated: true);

        await Assert.ThrowsAsync<ClinicalContextAccessDeniedException>(() => service.EnsureCanAccessCurrentClinicAsync());
    }

    [Fact]
    public async Task EnsureCanAccessCurrentClinicAsync_MissingClinicContext_ShouldFail()
    {
        var service = CreateService(new FakeUserClinicAccessRepository(), clinicId: null, userId: UserId, isAuthenticated: true);

        await Assert.ThrowsAsync<MissingClinicalContextException>(() => service.EnsureCanAccessCurrentClinicAsync());
    }

    [Fact]
    public async Task EnsureCanAccessClinicAsync_UnauthenticatedUser_ShouldFail()
    {
        var service = CreateService(new FakeUserClinicAccessRepository(), clinicId: 10, userId: UserId, isAuthenticated: false);

        await Assert.ThrowsAsync<CurrentUserResolutionException>(() => service.EnsureCanAccessClinicAsync(10));
    }

    [Fact]
    public async Task EnsureCanAccessClinicAsync_ExplicitClinicId_ShouldValidateExplicitClinic()
    {
        var repository = new FakeUserClinicAccessRepository((UserId, 20, true));
        var service = CreateService(repository, clinicId: 10, userId: UserId, isAuthenticated: true);

        await service.EnsureCanAccessClinicAsync(20);

        Assert.Equal(20, repository.LastClinicId);
    }

    [Fact]
    public async Task EnsureCanAccessClinicAsync_InvalidExplicitClinicId_ShouldFail()
    {
        var service = CreateService(new FakeUserClinicAccessRepository(), clinicId: 10, userId: UserId, isAuthenticated: true);

        await Assert.ThrowsAsync<InvalidClinicalContextException>(() => service.EnsureCanAccessClinicAsync(0));
    }

    private static ClinicalContextAuthorizationService CreateService(
        FakeUserClinicAccessRepository repository,
        long? clinicId,
        Guid userId,
        bool isAuthenticated)
    {
        return new ClinicalContextAuthorizationService(
            new FakeCurrentClinicalContext(clinicId),
            new FakeCurrentUserService(new CurrentUserInfo(userId, Profile: "Veterinarian", isAuthenticated)),
            repository);
    }

    private sealed class FakeCurrentClinicalContext : ICurrentClinicalContext
    {
        public FakeCurrentClinicalContext(long? clinicId) => ClinicId = clinicId;
        public long? ClinicId { get; }
        public bool HasClinic => ClinicId.HasValue;
        public long GetRequiredClinicId() => ClinicId ?? throw new MissingClinicalContextException();
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private readonly CurrentUserInfo _currentUser;
        public FakeCurrentUserService(CurrentUserInfo currentUser) => _currentUser = currentUser;
        public CurrentUserInfo GetCurrentUser() => _currentUser;
    }

    private sealed class FakeUserClinicAccessRepository : IUserClinicAccessRepository
    {
        private readonly List<(Guid UserId, long ClinicId, bool IsActive)> _items;
        public FakeUserClinicAccessRepository(params (Guid UserId, long ClinicId, bool IsActive)[] items) => _items = items.ToList();
        public long? LastClinicId { get; private set; }

        public Task<bool> HasActiveAccessAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default)
        {
            LastClinicId = clinicId;
            return Task.FromResult(_items.Any(item => item.UserId == userId && item.ClinicId == clinicId && item.IsActive));
        }

        public Task<UserClinicAccess?> GetAsync(Guid userId, long clinicId, CancellationToken cancellationToken = default) => Task.FromResult<UserClinicAccess?>(null);
    }
}
