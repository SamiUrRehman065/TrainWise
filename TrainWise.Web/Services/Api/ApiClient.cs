using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TrainWise.Web.Services.State;

namespace TrainWise.Web.Services.Api;

/// <summary>
/// Consolidated API client with session-based auth (X-Session-Token),
/// error handling, logging, and 401 session clearing.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionState _sessionState;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ApiClient> _logger;

    /// <summary>
    /// Shared JSON options with case-insensitive deserialization.
    /// The API returns camelCase (default ASP.NET Core behavior),
    /// while frontend models use PascalCase C# conventions.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, SessionState sessionState, IJSRuntime jsRuntime, ILogger<ApiClient> logger)
    {
        _http = http;
        _sessionState = sessionState;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    private async Task ClearInvalidSessionAsync(string method, string path)
    {
        _logger.LogWarning("Unauthorized access on {Method} {Path}", method, path);
        _sessionState.ClearSession();
        await _sessionState.ClearStorageAsync(_jsRuntime);
    }

    private void ApplySessionHeader()
    {
        _http.DefaultRequestHeaders.Remove("X-Session-Token");
        if (_sessionState.IsAuthenticated && _sessionState.SessionToken is not null)
        {
            _http.DefaultRequestHeaders.Add("X-Session-Token", _sessionState.SessionToken);
        }
    }

    public async Task<T?> GetAsync<T>(string path) where T : class
    {
        try
        {
            ApplySessionHeader();
            var response = await _http.GetAsync(path);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("GET", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API error: {StatusCode} - GET {Path}", response.StatusCode, path);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Path}", path);
            return null;
        }
    }

    public async Task<T?> PostAsync<T, TRequest>(string path, TRequest body) where T : class
    {
        try
        {
            ApplySessionHeader();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _http.PostAsync(path, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("POST", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API error: {StatusCode} - POST {Path}", response.StatusCode, path);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Path}", path);
            return null;
        }
    }

    public async Task<T?> PostMultipartAsync<T>(string path, MultipartFormDataContent content) where T : class
    {
        try
        {
            ApplySessionHeader();
            var response = await _http.PostAsync(path, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("POST (multipart)", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API error: {StatusCode} - POST (multipart) {Path}", response.StatusCode, path);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST (multipart) {Path}", path);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string path)
    {
        try
        {
            ApplySessionHeader();
            var response = await _http.DeleteAsync(path);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("DELETE", path);
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE {Path}", path);
            return false;
        }
    }

    public async Task<byte[]?> GetByteArrayAsync(string path)
    {
        try
        {
            ApplySessionHeader();
            var response = await _http.GetAsync(path);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("GET (bytes)", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET (bytes) {Path}", path);
            return null;
        }
    }

    public async Task<T?> PutAsync<T, TRequest>(string path, TRequest body) where T : class
    {
        try
        {
            ApplySessionHeader();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _http.PutAsync(path, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await ClearInvalidSessionAsync("PUT", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API error: {StatusCode} - PUT {Path}", response.StatusCode, path);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return string.IsNullOrEmpty(responseJson) ? null : JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PUT {Path}", path);
            return null;
        }
    }
}
