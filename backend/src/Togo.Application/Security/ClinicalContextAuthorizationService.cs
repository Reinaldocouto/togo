namespace Togo.Application.Security;

public sealed class ClinicalContextAuthorizationService : IClinicalContextAuthorizationService
{
    private readonly ICurrentClinicalContext _currentClinicalContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserClinicAccessRepository _userClinicAccessRepository;

    public ClinicalContextAuthorizationService(
        ICurrentClinicalContext currentClinicalContext,
        ICurrentUserService currentUserService,
        IUserClinicAccessRepository userClinicAccessRepository)
    {
        _currentClinicalContext = currentClinicalContext;
        _currentUserService = currentUserService;
        _userClinicAccessRepository = userClinicAccessRepository;
    }

    public Task EnsureCanAccessCurrentClinicAsync(CancellationToken cancellationToken = default)
    {
        var clinicId = _currentClinicalContext.GetRequiredClinicId();
        return EnsureCanAccessClinicAsync(clinicId, cancellationToken);
    }

    public async Task EnsureCanAccessClinicAsync(long clinicId, CancellationToken cancellationToken = default)
    {
        if (clinicId <= 0)
        {
            throw new InvalidClinicalContextException("ClinicId must be greater than zero.");
        }

        var currentUser = _currentUserService.GetCurrentUser();
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
        {
            throw new CurrentUserResolutionException("An authenticated current user is required.");
        }

        var hasAccess = await _userClinicAccessRepository.HasActiveAccessAsync(currentUser.UserId, clinicId, cancellationToken);
        if (!hasAccess)
        {
            throw new ClinicalContextAccessDeniedException(currentUser.UserId, clinicId);
        }
    }
}
