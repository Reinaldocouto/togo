using Togo.Domain.Entities;

namespace Togo.Domain.Interfaces;

public interface IAuthenticateUser
{
    Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
