using TrainWise.Web.Models;

namespace TrainWise.Web.Services.Api;

public sealed class TrainingApi
{
    private readonly ApiClient _apiClient;

    public TrainingApi(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<TrainResult?> TrainAsync(TrainRequest request)
    {
        return await _apiClient.PostAsync<TrainResult, TrainRequest>("api/train", request);
    }
}
