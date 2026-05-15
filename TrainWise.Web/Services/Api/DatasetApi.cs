using Microsoft.AspNetCore.Components.Forms;
using TrainWise.Web.Models;

namespace TrainWise.Web.Services.Api;

public sealed class DatasetApi
{
    private readonly ApiClient _apiClient;

    public DatasetApi(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<DatasetUploadResponse?> UploadAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
        content.Add(new StreamContent(stream), "file", file.Name);

        return await _apiClient.PostMultipartAsync<DatasetUploadResponse>("api/dataset/upload", content);
    }

    public async Task<DatasetSummary?> GetSummaryAsync(Guid datasetId)
    {
        return await _apiClient.GetAsync<DatasetSummary>($"api/dataset/{datasetId}/summary");
    }


    public async Task<DatasetIntelligence?> GetIntelligenceAsync(Guid datasetId, string? targetColumn = null)
    {
        var qs = string.IsNullOrWhiteSpace(targetColumn) ? "" : $"?targetColumn={Uri.EscapeDataString(targetColumn)}";
        return await _apiClient.GetAsync<DatasetIntelligence>($"api/dataset/{datasetId}/intelligence{qs}");
    }

    public async Task<DatasetList?> GetAllAsync()
    {
        return await _apiClient.GetAsync<DatasetList>("api/dataset");
    }

    public async Task<bool> DeleteAsync(Guid datasetId)
    {
        return await _apiClient.DeleteAsync($"api/dataset/{datasetId}");
    }

    /// <summary>
    /// Get storage and usage stats for the current user.
    /// GET /api/dataset/manage/stats
    /// </summary>
    public async Task<DashboardStats?> GetStatsAsync()
    {
        return await _apiClient.GetAsync<DashboardStats>("api/dataset/manage/stats");
    }

    /// <summary>
    /// Archive old datasets (older than specified days).
    /// POST /api/dataset/manage/archive?daysOld={daysOld}
    /// </summary>
    public async Task<bool> ArchiveAsync(int daysOld = 30)
    {
        var result = await _apiClient.PostAsync<object>($"api/dataset/manage/archive?daysOld={daysOld}", new { });
        return result is not null;
    }

    /// <summary>
    /// Get current storage mode (Disk or SqlBlob).
    /// GET /api/dataset/manage/storage-mode
    /// </summary>
    public async Task<string?> GetStorageModeAsync()
    {
        var result = await _apiClient.GetAsync<StorageModeResponse>("api/dataset/manage/storage-mode");
        return result?.StorageMode;
    }

    private sealed class StorageModeResponse
    {
        public string StorageMode { get; set; } = string.Empty;
    }
}
