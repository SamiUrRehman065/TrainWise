using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainWise.API.Contracts.Training;
using TrainWise.API.Data;
using TrainWise.API.Data.Models;
using TrainWise.API.Services.ML;

namespace TrainWise.API.Services.Training;

public sealed class TrainingService : ITrainingService
{
    private readonly AppDbContext _dbContext;
    private readonly IMLServiceClient _mlServiceClient;

    public TrainingService(AppDbContext dbContext, IMLServiceClient mlServiceClient)
    {
        _dbContext = dbContext;
        _mlServiceClient = mlServiceClient;
    }

    public async Task<TrainResultDto?> TrainAsync(TrainRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.DatasetId, out var datasetId))
        {
            return null;
        }

        var dataset = await _dbContext.Datasets
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DatasetId == datasetId && d.UserId == userId, cancellationToken);

        if (dataset is null)
        {
            return null;
        }

        // Pass the file path so ML service doesn't depend on its in-memory registry
        request.FilePath = dataset.FilePath;

        var result = await _mlServiceClient.TrainAsync(request, cancellationToken);
        if (result is null)
        {
            return null;
        }

        var experimentId = Guid.NewGuid();
        result.ExperimentId = experimentId.ToString();

        var experiment = new Experiment
        {
            ExperimentId = experimentId,
            DatasetId = datasetId,
            ModelName = result.ModelName,
            TaskType = result.TaskType,
            PreprocessingJson = JsonSerializer.Serialize(request.Preprocessing),
            HyperparametersJson = JsonSerializer.Serialize(request.Model.Hyperparameters),
            MetricsJson = JsonSerializer.Serialize(result),
            TrainingDurationSec = result.TrainingDurationSeconds,
            ModelPath = result.ModelPath,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Experiments.Add(experiment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }
}
