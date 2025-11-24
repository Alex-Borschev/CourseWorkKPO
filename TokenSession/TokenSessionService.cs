using System.Collections.Concurrent;

namespace TokenServiceLibrary;

public class TokenSessionService
{
    private readonly ConcurrentDictionary<string, SessionData> _sessions
        = new ConcurrentDictionary<string, SessionData>();

    private readonly TimeSpan _sessionLifetime;

    public TokenSessionService(TimeSpan sessionLifetime)
    {
        _sessionLifetime = sessionLifetime;
    }

    public string CreateSession(string userId)
    {
        string token = Guid.NewGuid().ToString("N");

        var data = new SessionData
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_sessionLifetime)
        };

        _sessions[token] = data;

        return token;
    }

    public SessionData ValidateToken(string token)
    {
        if (token == null) return null;

        if (_sessions.TryGetValue(token, out var session))
        {
            if (session.ExpiresAt > DateTime.UtcNow)
            {
                // обновляем время, чтобы сессия оставалась активной
                session.ExpiresAt = DateTime.UtcNow.Add(_sessionLifetime);
                return session;
            }
            else
            {
                // срок вышел — удаляем
                _sessions.TryRemove(token, out _);
            }
        }

        return null; // токен невалидный
    }

    public void RemoveSession(string token)
    {
        if (token == null) return;
        _sessions.TryRemove(token, out _);
    }

    public void CleanupExpired()
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _sessions)
        {
            if (kvp.Value.ExpiresAt <= now)
            {
                _sessions.TryRemove(kvp.Key, out _);
            }
        }
    }
}
