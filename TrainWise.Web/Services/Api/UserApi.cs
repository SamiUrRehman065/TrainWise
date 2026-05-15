using TrainWise.Web.Models;

namespace TrainWise.Web.Services.Api;

public sealed class UserApi
{
    private readonly ApiClient _api;

    public UserApi(ApiClient api)
    {
        _api = api;
    }

    public async Task<UserSettingsResponse?> GetSettingsAsync()
    {
        return await _api.GetAsync<UserSettingsResponse>("api/user/settings");
    }

    public async Task<bool> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        var response = await _api.PutAsync<object, UpdateSettingsRequest>("api/user/settings", request);
        return response != null;
    }

    public async Task<bool> DeactivateWorkspaceAsync()
    {
        var response = await _api.PostAsync<object, object>("api/user/deactivate", new { });
        return response != null;
    }
}
