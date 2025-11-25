using Togo.Domain.Entities;

namespace Togo.Application.Interfaces;

public interface ITokenService
{
    string IssueToken(User user);
    bool TryValidateToken(string token, out Guid userId);
}
