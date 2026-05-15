using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Contracts.Training;

namespace TrainWise.API.Services.ML;

public interface IMLServiceClient
{
    Task<DatasetSummaryDto?> AnalyzeAsync(string filePath, Guid datasetId, CancellationToken cancellationToken);
    Task<EdaReportDto?> EdaAsync(Guid datasetId, string? targetColumn, string? filePath, CancellationToken cancellationToken);
    Task<DatasetIntelligenceDto?> GetIntelligenceAsync(Guid datasetId, string? targetColumn, string? filePath, CancellationToken cancellationToken);
    Task<TrainResultDto?> TrainAsync(TrainRequest request, CancellationToken cancellationToken);
    Task<List<RecommendationDto>?> RecommendAsync(object metrics, object config, CancellationToken cancellationToken);
    Task<bool> HealthAsync(CancellationToken cancellationToken);
}
