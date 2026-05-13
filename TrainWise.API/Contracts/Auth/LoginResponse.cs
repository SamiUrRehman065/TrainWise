namespace TrainWise.API.Contracts.Auth;

public sealed class LoginResponse
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
