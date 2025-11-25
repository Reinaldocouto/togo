using Togo.Application.Interfaces;
using Togo.Domain.Entities;

namespace Togo.Application.Services;

public class AuthenticateUserService : IAuthenticateUser
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticateUserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (!User.IsEmailValid(email))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return _passwordHasher.VerifyPassword(password, user.PasswordHash) ? user : null;
    }
}
