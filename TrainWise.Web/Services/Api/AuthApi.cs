using Microsoft.JSInterop;
using System.Text.Json.Serialization;
using TrainWise.Web.Services.State;

namespace TrainWise.Web.Services.Api;

public sealed class AuthApi
{
    private readonly ApiClient _apiClient;
    private readonly SessionState _sessionState;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthApi> _logger;

    public AuthApi(ApiClient apiClient, SessionState sessionState, IJSRuntime jsRuntime, ILogger<AuthApi> logger)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _apiClient.PostAsync<LoginResponse, LoginRequest>("api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        });

        if (response is null)
        {
            _logger.LogWarning("Login failed: API returned null response for user '{User}'", username);
            return false;
        }

        if (string.IsNullOrWhiteSpace(response.SessionToken))
        {
            _logger.LogError("Login failed: SessionToken is empty after deserialization — JSON property name mismatch");
            return false;
        }

        _logger.LogInformation("Login successful for user '{User}'", username);
        _sessionState.SetSession(response.SessionToken, response.UserId, username);
        await _sessionState.PersistToStorageAsync(_jsRuntime);

        return true;
    }

    public async Task<bool> SignupAsync(string username, string password)
    {
        var response = await _apiClient.PostAsync<LoginResponse, SignupRequest>("api/auth/signup", new SignupRequest
        {
            Username = username,
            Password = password
        });

        if (response is null)
        {
            _logger.LogWarning("Signup failed: API returned null response for user '{User}'", username);
            return false;
        }

        if (string.IsNullOrWhiteSpace(response.SessionToken))
        {
            _logger.LogError("Signup failed: SessionToken is empty after deserialization — JSON property name mismatch");
            return false;
        }

        _logger.LogInformation("Signup successful for user '{User}' (SessionToken length: {Len})", username, response.SessionToken.Length);
        _sessionState.SetSession(response.SessionToken, response.UserId, username);
        await _sessionState.PersistToStorageAsync(_jsRuntime);
        return true;
    }

    private sealed class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class SignupRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        [JsonPropertyName("sessionToken")]
        public string SessionToken { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
    }
}
