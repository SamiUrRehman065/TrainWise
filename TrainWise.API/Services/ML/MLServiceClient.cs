using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainWise.API.Configuration;
using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Contracts.Training;

namespace TrainWise.API.Services.ML;

public sealed class MLServiceClient : IMLServiceClient
{
    private readonly HttpClient _httpClient;

    public MLServiceClient(HttpClient httpClient, IOptions<MLServiceOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<DatasetSummaryDto?> AnalyzeAsync(string filePath, Guid datasetId, CancellationToken cancellationToken)
    {
        var payload = new { filePath, datasetId };
        var response = await _httpClient.PostAsJsonAsync("/analyze", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DatasetSummaryDto>(cancellationToken: cancellationToken);
    }

    public async Task<EdaReportDto?> EdaAsync(Guid datasetId, string? targetColumn, string? filePath, CancellationToken cancellationToken)
    {
        var payload = new { datasetId, targetColumn, filePath };
        var response = await _httpClient.PostAsJsonAsync("/eda", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<EdaReportDto>(cancellationToken: cancellationToken);
    }

    public async Task<DatasetIntelligenceDto?> GetIntelligenceAsync(Guid datasetId, string? targetColumn, string? filePath, CancellationToken cancellationToken)
    {
        var payload = new { datasetId, targetColumn, filePath };
        var response = await _httpClient.PostAsJsonAsync("/intelligence", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DatasetIntelligenceDto>(cancellationToken: cancellationToken);
    }

    public async Task<TrainResultDto?> TrainAsync(TrainRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("/train", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TrainResultDto>(cancellationToken: cancellationToken);
    }

    public async Task<List<RecommendationDto>?> RecommendAsync(object metrics, object config, CancellationToken cancellationToken)
    {
        var payload = new { metrics, config };
        var response = await _httpClient.PostAsJsonAsync("/recommend", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<List<RecommendationDto>>(cancellationToken: cancellationToken);
    }

    public async Task<bool> HealthAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("/health", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
