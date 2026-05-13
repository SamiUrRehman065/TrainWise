using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Contracts.Training;

namespace TrainWise.API.Services.ML;

public interface IMLServiceClient
{
    Task<DatasetSummaryDto?> AnalyzeAsync(string filePath, Guid datasetId, CancellationToken cancellationToken);
    Task<TrainResultDto?> TrainAsync(TrainRequest request, CancellationToken cancellationToken);
    Task<List<RecommendationDto>?> RecommendAsync(object metrics, object config, CancellationToken cancellationToken);
    Task<bool> HealthAsync(CancellationToken cancellationToken);
}
