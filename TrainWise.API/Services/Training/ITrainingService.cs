using TrainWise.API.Contracts.Training;

namespace TrainWise.API.Services.Training;

public interface ITrainingService
{
    Task<TrainResultDto?> TrainAsync(TrainRequest request, Guid userId, CancellationToken cancellationToken);
}
