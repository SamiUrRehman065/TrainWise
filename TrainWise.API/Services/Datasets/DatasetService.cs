using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrainWise.API.Configuration;
using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Data;
using TrainWise.API.Data.Models;
using TrainWise.API.Services.ML;

namespace TrainWise.API.Services.Datasets;

public sealed class DatasetService : IDatasetService
{
    private readonly AppDbContext _dbContext;
    private readonly UploadOptions _uploadOptions;
    private readonly IMLServiceClient _mlServiceClient;

    public DatasetService(
        AppDbContext dbContext,
        IOptions<UploadOptions> uploadOptions,
        IMLServiceClient mlServiceClient)
    {
        _dbContext = dbContext;
        _uploadOptions = uploadOptions.Value;
        _mlServiceClient = mlServiceClient;
    }

    public async Task<DatasetUploadResponse> UploadAsync(IFormFile file, Guid userId, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("File is required.");
        }

        var maxBytes = _uploadOptions.MaxFileSizeMb * 1024L * 1024L;
        if (file.Length > maxBytes)
        {
            throw new InvalidOperationException("File exceeds the maximum allowed size.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".xlsx")
        {
            throw new InvalidOperationException("Only CSV and XLSX files are supported.");
        }

        var fileHash = await ComputeFileHashAsync(file, cancellationToken);
        var existing = await _dbContext.Datasets
            .AsNoTracking()
            .Where(d => d.UserId == userId && d.FileHash == fileHash)
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return new DatasetUploadResponse
            {
                DatasetId = existing.DatasetId,
                FileName = existing.FileName,
                RowCount = existing.RowCount,
                IsDuplicate = true
            };
        }

        var datasetId = Guid.NewGuid();
        var datasetDirectory = Path.Combine(_uploadOptions.StoragePath, userId.ToString(), datasetId.ToString());
        Directory.CreateDirectory(datasetDirectory);

        var sanitizedFileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(datasetDirectory, sanitizedFileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var dataset = new Dataset
        {
            DatasetId = datasetId,
            UserId = userId,
            FileName = sanitizedFileName,
            FilePath = filePath,
            FileHash = fileHash,
            RowCount = 0,
            ColumnCount = 0,
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.Datasets.Add(dataset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var summary = await _mlServiceClient.AnalyzeAsync(filePath, datasetId, cancellationToken);
        if (summary is not null)
        {
            dataset.AnalysisSummaryJson = JsonSerializer.Serialize(summary);
            dataset.RowCount = summary.RowCount;
            dataset.ColumnCount = summary.ColumnCount;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new DatasetUploadResponse
        {
            DatasetId = datasetId,
            FileName = sanitizedFileName,
            RowCount = summary?.RowCount ?? dataset.RowCount,
            IsDuplicate = false
        };
    }

    public async Task<DatasetListDto> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var datasets = await _dbContext.Datasets
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

        var items = datasets.Select(d =>
        {
            long size = 0;
            if (!string.IsNullOrWhiteSpace(d.FilePath) && File.Exists(d.FilePath))
            {
                try { size = new FileInfo(d.FilePath).Length; }
                catch { }
            }

            return new DatasetListItemDto
            {
                DatasetId = d.DatasetId,
                FileName = d.FileName,
                RowCount = d.RowCount,
                ColumnCount = d.ColumnCount,
                Size = size,
                UploadedAt = d.UploadedAt
            };
        }).ToList();

        return new DatasetListDto
        {
            Items = items
        };
    }

    public async Task<DatasetSummaryDto?> GetSummaryAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken)
    {
        var dataset = await _dbContext.Datasets
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DatasetId == datasetId && d.UserId == userId, cancellationToken);

        if (dataset is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dataset.AnalysisSummaryJson))
        {
            return JsonSerializer.Deserialize<DatasetSummaryDto>(dataset.AnalysisSummaryJson);
        }

        var summary = await _mlServiceClient.AnalyzeAsync(dataset.FilePath, datasetId, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        var tracked = await _dbContext.Datasets
            .FirstAsync(d => d.DatasetId == datasetId, cancellationToken);
        tracked.AnalysisSummaryJson = JsonSerializer.Serialize(summary);
        tracked.RowCount = summary.RowCount;
        tracked.ColumnCount = summary.ColumnCount;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return summary;
    }

    public async Task<bool> DeleteAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken)
    {
        var dataset = await _dbContext.Datasets
            .Include(d => d.Experiments)
            .FirstOrDefaultAsync(d => d.DatasetId == datasetId && d.UserId == userId, cancellationToken);

        if (dataset is null)
        {
            return false;
        }

        if (dataset.Experiments.Count > 0)
        {
            _dbContext.Experiments.RemoveRange(dataset.Experiments);
        }

        _dbContext.Datasets.Remove(dataset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var datasetDirectory = Path.GetDirectoryName(dataset.FilePath);
        if (!string.IsNullOrWhiteSpace(datasetDirectory) && Directory.Exists(datasetDirectory))
        {
            try
            {
                Directory.Delete(datasetDirectory, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return true;
    }

    private static async Task<string> ComputeFileHashAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
