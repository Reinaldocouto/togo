using System.Collections.Concurrent;
using Togo.Domain.Interfaces;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Tokens;

public class InMemoryTokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, Guid> _tokens = new();

    public string IssueToken(User user)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = user.Id;
        return token;
    }

    public bool TryValidateToken(string token, out Guid userId)
    {
        return _tokens.TryGetValue(token, out userId);
    }
}
