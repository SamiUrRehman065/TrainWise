using System.Collections.Concurrent;

namespace TrainWise.API.Services.Auth;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new();
    private DateTimeOffset _lastCleanup = DateTimeOffset.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30);

    public string CreateSession(Guid userId, TimeSpan ttl)
    {
        CleanupExpiredIfNeeded();

        var token = Guid.NewGuid().ToString("N");
        var entry = new SessionEntry(userId, DateTimeOffset.UtcNow.Add(ttl));
        _sessions[token] = entry;
        return token;
    }

    public bool TryGetUserId(string token, out Guid userId)
    {
        userId = Guid.Empty;

        if (!_sessions.TryGetValue(token, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        userId = entry.UserId;
        return true;
    }

    public void RemoveSession(string token)
    {
        _sessions.TryRemove(token, out _);
    }

    public int ActiveSessionCount => _sessions.Count;

    /// <summary>
    /// Periodically removes expired sessions to prevent memory leaks.
    /// Runs at most once every 30 minutes.
    /// </summary>
    private void CleanupExpiredIfNeeded()
    {
        if (DateTimeOffset.UtcNow - _lastCleanup < CleanupInterval)
        {
            return;
        }

        _lastCleanup = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;
        var expired = _sessions.Where(kvp => kvp.Value.ExpiresAt <= now).Select(kvp => kvp.Key).ToList();

        foreach (var key in expired)
        {
            _sessions.TryRemove(key, out _);
        }
    }

    private sealed record SessionEntry(Guid UserId, DateTimeOffset ExpiresAt);
}
