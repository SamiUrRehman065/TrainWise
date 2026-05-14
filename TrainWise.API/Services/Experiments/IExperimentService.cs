using TrainWise.API.Contracts.Experiments;
using TrainWise.API.Contracts.Training;

namespace TrainWise.API.Services.Experiments;

public interface IExperimentService
{
    Task<ExperimentDetailDto?> GetAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken);
    Task<ExperimentListDto> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken);
    Task<List<RecommendationDto>?> GetRecommendationsAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken);
}
