using Togo.Domain.Entities;

namespace Togo.Application.Interfaces;

public interface IAuthenticateUser
{
    Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
