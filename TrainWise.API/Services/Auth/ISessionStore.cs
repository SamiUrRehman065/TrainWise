namespace TrainWise.API.Services.Auth;

public interface ISessionStore
{
    string CreateSession(Guid userId, TimeSpan ttl);
    bool TryGetUserId(string token, out Guid userId);
    void RemoveSession(string token);
}
