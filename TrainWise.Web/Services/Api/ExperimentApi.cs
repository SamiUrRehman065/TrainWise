using TrainWise.Web.Models;

namespace TrainWise.Web.Services.Api;

public sealed class ExperimentApi
{
    private readonly ApiClient _apiClient;

    public ExperimentApi(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ExperimentDetail?> GetAsync(Guid experimentId)
    {
        return await _apiClient.GetAsync<ExperimentDetail>($"api/experiment/{experimentId}");
    }

    public async Task<ExperimentList?> GetHistoryAsync(int page = 1, int pageSize = 20)
    {
        return await _apiClient.GetAsync<ExperimentList>($"api/experiment/history?page={page}&pageSize={pageSize}");
    }

    public async Task<bool> DeleteAsync(Guid experimentId)
    {
        return await _apiClient.DeleteAsync($"api/experiment/{experimentId}");
    }

    /// <summary>
    /// Get training history across all datasets for the current user.
    /// GET /api/dataset/manage/training-history
    /// </summary>
    public async Task<TrainingHistoryResponse?> GetTrainingHistoryAsync()
    {
        return await _apiClient.GetAsync<TrainingHistoryResponse>("api/dataset/manage/training-history");
    }
}
