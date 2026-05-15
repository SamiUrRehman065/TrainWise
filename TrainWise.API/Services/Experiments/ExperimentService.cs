using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainWise.API.Contracts.Experiments;
using TrainWise.API.Contracts.Training;
using TrainWise.API.Data;
using TrainWise.API.Services.ML;

namespace TrainWise.API.Services.Experiments;

public sealed class ExperimentService : IExperimentService
{
    private readonly AppDbContext _dbContext;
    private readonly IMLServiceClient _mlServiceClient;

    public ExperimentService(AppDbContext dbContext, IMLServiceClient mlServiceClient)
    {
        _dbContext = dbContext;
        _mlServiceClient = mlServiceClient;
    }

    public async Task<ExperimentDetailDto?> GetAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken)
    {
        var experiment = await _dbContext.Experiments
            .Include(e => e.Dataset)
            .FirstOrDefaultAsync(e => e.ExperimentId == experimentId && e.Dataset!.UserId == userId, cancellationToken);

        if (experiment is null)
        {
            return null;
        }

        return new ExperimentDetailDto
        {
            ExperimentId = experiment.ExperimentId,
            DatasetId = experiment.DatasetId,
            DatasetName = experiment.Dataset?.FileName ?? string.Empty,
            ModelName = experiment.ModelName,
            TaskType = experiment.TaskType,
            PreprocessingJson = experiment.PreprocessingJson,
            HyperparametersJson = experiment.HyperparametersJson,
            MetricsJson = experiment.MetricsJson,
            TrainingDurationSec = experiment.TrainingDurationSec,
            ModelPath = experiment.ModelPath,
            CreatedAt = experiment.CreatedAt
        };
    }

    public async Task<Stream?> DownloadModelAsync(string modelPath, CancellationToken cancellationToken)
    {
        return await _mlServiceClient.DownloadModelAsync(modelPath, cancellationToken);
    }

    public async Task<List<RecommendationDto>?> GetRecommendationsAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken)
    {
        var experiment = await _dbContext.Experiments
            .Include(e => e.Dataset)
            .FirstOrDefaultAsync(e => e.ExperimentId == experimentId && e.Dataset!.UserId == userId, cancellationToken);

        if (experiment is null)
            return null;

        object metrics = string.IsNullOrWhiteSpace(experiment.MetricsJson) 
            ? new object() 
            : JsonSerializer.Deserialize<object>(experiment.MetricsJson)!;

        var config = new {
            preprocessing = string.IsNullOrWhiteSpace(experiment.PreprocessingJson) ? new object() : JsonSerializer.Deserialize<object>(experiment.PreprocessingJson)!,
            model = new {
                name = experiment.ModelName,
                hyperparameters = string.IsNullOrWhiteSpace(experiment.HyperparametersJson) ? new object() : JsonSerializer.Deserialize<object>(experiment.HyperparametersJson)!
            },
            crossValidation = false
        };

        object? analysis = string.IsNullOrWhiteSpace(experiment.Dataset?.AnalysisSummaryJson)
            ? null
            : JsonSerializer.Deserialize<object>(experiment.Dataset.AnalysisSummaryJson)!;

        return await _mlServiceClient.RecommendAsync(metrics, config, analysis, cancellationToken);
    }

    public async Task<ExperimentListDto> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Experiments
            .Include(e => e.Dataset)
            .Where(e => e.Dataset!.UserId == userId)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new ExperimentListDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        foreach (var item in items)
        {
            var metricsSummary = ExtractMetricsSummary(item.MetricsJson);

            result.Items.Add(new ExperimentListItemDto
            {
                ExperimentId = item.ExperimentId,
                DatasetId = item.DatasetId,
                DatasetName = item.Dataset?.FileName ?? string.Empty,
                ModelName = item.ModelName,
                TaskType = item.TaskType,
                MetricsSummary = metricsSummary,
                PreprocessingSummary = item.PreprocessingJson,
                CreatedAt = item.CreatedAt
            });
        }

        return result;
    }

    private static string ExtractMetricsSummary(string metricsJson)
    {
        if (string.IsNullOrWhiteSpace(metricsJson))
            return "No metrics";

        try
        {
            using var doc = JsonDocument.Parse(metricsJson);
            var root = doc.RootElement;

            JsonElement metricsObj;
            if (root.TryGetProperty("ClassificationMetrics", out metricsObj) ||
                root.TryGetProperty("classificationMetrics", out metricsObj))
            {
                var parts = new List<string>();

                if (metricsObj.TryGetProperty("Accuracy", out var acc) ||
                    metricsObj.TryGetProperty("accuracy", out acc))
                    parts.Add($"Acc: {acc.GetDouble():0.0%}");

                if (metricsObj.TryGetProperty("F1Score", out var f1) ||
                    metricsObj.TryGetProperty("f1Score", out f1) ||
                    metricsObj.TryGetProperty("f1_score", out f1))
                    parts.Add($"F1: {f1.GetDouble():0.000}");

                if (parts.Count > 0)
                    return string.Join(" | ", parts);
            }

            if (root.TryGetProperty("RegressionMetrics", out var reg) ||
                root.TryGetProperty("regressionMetrics", out reg))
            {
                var parts = new List<string>();

                if (reg.TryGetProperty("R2Score", out var r2) ||
                    reg.TryGetProperty("r2Score", out r2) ||
                    reg.TryGetProperty("r2_score", out r2))
                    parts.Add($"R²: {r2.GetDouble():0.000}");

                if (reg.TryGetProperty("Rmse", out var rmse) ||
                    reg.TryGetProperty("rmse", out rmse))
                    parts.Add($"RMSE: {rmse.GetDouble():0.000}");

                if (parts.Count > 0)
                    return string.Join(" | ", parts);
            }

            if (root.TryGetProperty("Accuracy", out var flatAcc) ||
                root.TryGetProperty("accuracy", out flatAcc))
                return $"Acc: {flatAcc.GetDouble():0.0%}";
        }
        catch
        {
        }

        return "Metrics available";
    }

    public async Task<bool> DeleteAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken)
    {
        var experiment = await _dbContext.Experiments
            .Include(e => e.Dataset)
            .FirstOrDefaultAsync(e => e.ExperimentId == experimentId && e.Dataset!.UserId == userId, cancellationToken);

        if (experiment is null)
        {
            return false;
        }

        _dbContext.Experiments.Remove(experiment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
