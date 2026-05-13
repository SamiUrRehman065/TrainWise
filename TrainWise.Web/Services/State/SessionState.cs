using Microsoft.JSInterop;

namespace TrainWise.Web.Services.State;

public sealed class SessionState
{
    private const string StorageKeyToken = "tw_session_token";
    private const string StorageKeyUserId = "tw_session_userid";
    private const string StorageKeyUsername = "tw_session_username";

    public event Action? OnChanged;

    public string? SessionToken { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(SessionToken);

    public void SetSession(string token, Guid userId, string? username = null)
    {
        SessionToken = token;
        UserId = userId;
        Username = username;
        NotifyChanged();
    }

    public async Task PersistToStorageAsync(IJSRuntime js)
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.setItem", StorageKeyToken, SessionToken ?? "");
            await js.InvokeVoidAsync("sessionStorage.setItem", StorageKeyUserId, UserId?.ToString() ?? "");
            await js.InvokeVoidAsync("sessionStorage.setItem", StorageKeyUsername, Username ?? "");
        }
        catch
        {
            // localStorage not available (e.g. prerendering)
        }
    }

    public async Task RestoreFromStorageAsync(IJSRuntime js)
    {
        try
        {
            var token = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKeyToken);
            var userIdStr = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKeyUserId);
            var username = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKeyUsername);

            if (!string.IsNullOrWhiteSpace(token))
            {
                SessionToken = token;
                Username = username;

                if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out var uid))
                {
                    UserId = uid;
                }
                NotifyChanged();
            }
        }
        catch
        {
            // localStorage not available (e.g. prerendering)
        }
    }

    public async Task ClearStorageAsync(IJSRuntime js)
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKeyToken);
            await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKeyUserId);
            await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKeyUsername);
        }
        catch
        {
            // localStorage not available
        }
    }

    public void Clear()
    {
        SessionToken = null;
        UserId = null;
        Username = null;
        NotifyChanged();
    }

    public void ClearSession()
    {
        Clear();
    }

    private void NotifyChanged() => OnChanged?.Invoke();
}
