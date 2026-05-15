using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Data;
using TrainWise.API.Services.Auth;
using TrainWise.API.Services.Datasets;
using TrainWise.API.Services.ML;

namespace TrainWise.API.Controllers.Datasets;

/// <summary>
/// Dataset management endpoints: stats, archival, training history.
/// </summary>
[ApiController]
[Route("api/dataset/manage")]
[Authorize]
public sealed class DatasetManagementController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IDatasetStorageService _storageService;
    private readonly ILogger<DatasetManagementController> _logger;

    public DatasetManagementController(
        AppDbContext dbContext,
        IDatasetStorageService storageService,
        ILogger<DatasetManagementController> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Get current storage mode.
    /// </summary>
    [HttpGet("storage-mode")]
    public ActionResult<StorageModeResponse> GetStorageMode()
    {
        var mode = _storageService.GetStorageMode();
        return Ok(new StorageModeResponse { StorageMode = mode });
    }

    /// <summary>
    /// Get storage stats for current user (total datasets, total size, etc).
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetUserStats(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null) return Unauthorized();

        var datasets = await _dbContext.Datasets
            .AsNoTracking()
            .Where(d => d.UserId == userId.Value)
            .Include(d => d.Experiments)
            .ToListAsync(cancellationToken);

        var experiments = datasets.SelectMany(d => d.Experiments).ToList();
        var totalSize = datasets.Sum(d => (long)d.RowCount * d.ColumnCount);

        double bestAccuracy = 0;
        foreach (var exp in experiments)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(exp.MetricsJson);
                if (doc.RootElement.TryGetProperty("classificationMetrics", out var metrics) || 
                    doc.RootElement.TryGetProperty("ClassificationMetrics", out metrics))
                {
                    if (metrics.TryGetProperty("accuracy", out var acc) || metrics.TryGetProperty("Accuracy", out acc))
                    {
                        var val = acc.GetDouble();
                        if (val > bestAccuracy) bestAccuracy = val;
                    }
                }
            }
            catch { /* Ignore invalid JSON */ }
        }

        var modelsTried = experiments.Select(e => e.ModelName).Distinct().Count();

        return Ok(new DashboardStatsResponse
        {
            TotalDatasets = datasets.Count,
            ActiveDatasets = datasets.Count(d => d.Experiments.Count > 0),
            TotalExperiments = experiments.Count,
            BestAccuracy = bestAccuracy,
            ModelsTried = modelsTried,
            EstimatedSize = totalSize,
            StorageMode = _storageService.GetStorageMode(),
            IsPremium = user.IsPremium,
            MemberSince = user.CreatedAt
        });
    }

    /// <summary>
    /// Archive old datasets for current user (older than N days).
    /// </summary>
    [HttpPost("archive")]
    public async Task<ActionResult<ArchiveResponse>> ArchiveOldUserDatasets(
        [FromQuery] int daysOld = 30,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (daysOld <= 0)
        {
            return BadRequest(new { error = "daysOld must be greater than 0." });
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var userDatasets = await _dbContext.Datasets
            .Where(d => d.UserId == userId.Value && d.UploadedAt < cutoffDate && d.Experiments.Count == 0)
            .ToListAsync(cancellationToken);

        if (userDatasets.Count == 0)
        {
            return Ok(new ArchiveResponse { ArchivedCount = 0, Message = "No datasets to archive." });
        }

        var archived = 0;
        foreach (var dataset in userDatasets)
        {
            try
            {
                await _storageService.DeleteAsync(dataset.DatasetId, cancellationToken);
                _dbContext.Datasets.Remove(dataset);
                archived++;
                _logger.LogInformation("Archived dataset {DatasetId} for user {UserId}", dataset.DatasetId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive dataset {DatasetId}", dataset.DatasetId);
            }
        }

        if (archived > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new ArchiveResponse
        {
            ArchivedCount = archived,
            Message = $"Archived {archived} dataset(s) older than {daysOld} days."
        });
    }

    /// <summary>
    /// Get training history for current user's datasets.
    /// </summary>
    [HttpGet("training-history")]
    public async Task<ActionResult<TrainingHistoryResponse>> GetTrainingHistory(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var experiments = await _dbContext.Experiments
            .AsNoTracking()
            .Include(e => e.Dataset)
            .Where(e => e.Dataset != null && e.Dataset.UserId == userId.Value)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new TrainingHistoryItem
            {
                ExperimentId = e.ExperimentId,
                DatasetId = e.DatasetId,
                ModelName = e.ModelName,
                TaskType = e.TaskType,
                CreatedAt = e.CreatedAt,
                TrainingDurationSec = e.TrainingDurationSec,
                DatasetName = e.Dataset != null ? e.Dataset.FileName : ""
            })
            .ToListAsync(cancellationToken);

        return Ok(new TrainingHistoryResponse { TrainingHistory = experiments });
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null)
        {
            return null;
        }

        return Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}

// ── Typed Response DTOs ──

public sealed class StorageModeResponse
{
    public string StorageMode { get; set; } = string.Empty;
}

public sealed class DashboardStatsResponse
{
    public int TotalDatasets { get; set; }
    public int ActiveDatasets { get; set; }
    public int TotalExperiments { get; set; }
    public double BestAccuracy { get; set; }
    public int ModelsTried { get; set; }
    public long EstimatedSize { get; set; }
    public string StorageMode { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public DateTime MemberSince { get; set; }
}

public sealed class ArchiveResponse
{
    public bool Success { get; set; } = true;
    public int ArchivedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class TrainingHistoryResponse
{
    public List<TrainingHistoryItem> TrainingHistory { get; set; } = new();
}

public sealed class TrainingHistoryItem
{
    public Guid ExperimentId { get; set; }
    public Guid DatasetId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double? TrainingDurationSec { get; set; }
    public string DatasetName { get; set; } = string.Empty;
}
