using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrainWise.API.Configuration;
using TrainWise.API.Data;
using TrainWise.API.Data.Models;

namespace TrainWise.API.Services.Datasets;

/// <summary>
/// Manages dataset file storage with support for disk and SQL blob backends.
/// </summary>
public sealed class DatasetStorageService : IDatasetStorageService
{
    private readonly AppDbContext _dbContext;
    private readonly UploadOptions _uploadOptions;
    private readonly DatasetStorageOptions _storageOptions;
    private readonly ILogger<DatasetStorageService> _logger;

    public DatasetStorageService(
        AppDbContext dbContext,
        IOptions<UploadOptions> uploadOptions,
        IOptions<DatasetStorageOptions> storageOptions,
        ILogger<DatasetStorageService> logger)
    {
        _dbContext = dbContext;
        _uploadOptions = uploadOptions.Value;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task SaveAsync(Guid datasetId, string fileName, Stream fileStream, CancellationToken cancellationToken)
    {
        if (_storageOptions.Mode.Equals("SqlBlob", StringComparison.OrdinalIgnoreCase))
        {
            await SaveToSqlBlobAsync(datasetId, fileStream, cancellationToken);
        }
        else
        {
            await SaveToDiskAsync(datasetId, fileName, fileStream, cancellationToken);
        }
    }

    public async Task<Stream?> GetAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        if (_storageOptions.Mode.Equals("SqlBlob", StringComparison.OrdinalIgnoreCase))
        {
            return await GetFromSqlBlobAsync(datasetId, cancellationToken);
        }
        else
        {
            return GetFromDisk(datasetId);
        }
    }

    public async Task DeleteAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        if (_storageOptions.Mode.Equals("SqlBlob", StringComparison.OrdinalIgnoreCase))
        {
            await DeleteFromSqlBlobAsync(datasetId, cancellationToken);
        }
        else
        {
            DeleteFromDisk(datasetId);
        }
    }

    public async Task<int> ArchiveOldAsync(int daysOld, CancellationToken cancellationToken)
    {
        if (_storageOptions.AutoArchiveAfterDays <= 0 || string.IsNullOrWhiteSpace(_storageOptions.ArchivePath))
        {
            _logger.LogInformation("Auto-archive disabled or archive path not configured.");
            return 0;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var datasetsToArchive = await _dbContext.Datasets
            .Where(d => d.UploadedAt < cutoffDate && d.Experiments.Count == 0)
            .ToListAsync(cancellationToken);

        if (datasetsToArchive.Count == 0)
        {
            _logger.LogInformation("No datasets to archive.");
            return 0;
        }

        Directory.CreateDirectory(_storageOptions.ArchivePath);

        foreach (var dataset in datasetsToArchive)
        {
            try
            {
                var sourceDir = Path.GetDirectoryName(dataset.FilePath);
                if (sourceDir is null || !Directory.Exists(sourceDir))
                {
                    continue;
                }

                var archiveDir = Path.Combine(_storageOptions.ArchivePath, dataset.UserId.ToString(), dataset.DatasetId.ToString());
                Directory.CreateDirectory(archiveDir);

                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var destFile = Path.Combine(archiveDir, Path.GetFileName(file));
                    File.Move(file, destFile, true);
                }

                Directory.Delete(sourceDir, true);
                _logger.LogInformation($"Archived dataset {dataset.DatasetId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to archive dataset {dataset.DatasetId}.");
            }
        }

        return datasetsToArchive.Count;
    }

    public string GetStorageMode() => _storageOptions.Mode;

    private async Task SaveToDiskAsync(Guid datasetId, string fileName, Stream fileStream, CancellationToken cancellationToken)
    {
        var dataset = await _dbContext.Datasets.FirstOrDefaultAsync(d => d.DatasetId == datasetId, cancellationToken);
        if (dataset is null)
        {
            throw new InvalidOperationException($"Dataset {datasetId} not found.");
        }

        var datasetDirectory = Path.GetDirectoryName(dataset.FilePath);
        if (string.IsNullOrWhiteSpace(datasetDirectory))
        {
            throw new InvalidOperationException("Invalid dataset file path.");
        }

        Directory.CreateDirectory(datasetDirectory);
        var filePath = Path.Combine(datasetDirectory, fileName);

        await using var fileStream2 = File.Create(filePath);
        await fileStream.CopyToAsync(fileStream2, cancellationToken);

        _logger.LogInformation($"Saved dataset {datasetId} to disk: {filePath}");
    }

    private async Task SaveToSqlBlobAsync(Guid datasetId, Stream fileStream, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var fileContent = memoryStream.ToArray();

        var blob = new DatasetBlob
        {
            BlobId = Guid.NewGuid(),
            DatasetId = datasetId,
            FileContent = fileContent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DatasetBlobs.Add(blob);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Saved dataset {datasetId} to SQL blob ({fileContent.Length} bytes).");
    }

    private Stream? GetFromDisk(Guid datasetId)
    {
        var dataset = _dbContext.Datasets.AsNoTracking().FirstOrDefault(d => d.DatasetId == datasetId);
        if (dataset is null || !File.Exists(dataset.FilePath))
        {
            return null;
        }

        return File.OpenRead(dataset.FilePath);
    }

    private async Task<Stream?> GetFromSqlBlobAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var blob = await _dbContext.DatasetBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.DatasetId == datasetId, cancellationToken);

        if (blob is null)
        {
            return null;
        }

        return new MemoryStream(blob.FileContent);
    }

    private void DeleteFromDisk(Guid datasetId)
    {
        var dataset = _dbContext.Datasets.AsNoTracking().FirstOrDefault(d => d.DatasetId == datasetId);
        if (dataset is null)
        {
            return;
        }

        var datasetDirectory = Path.GetDirectoryName(dataset.FilePath);
        if (!string.IsNullOrWhiteSpace(datasetDirectory) && Directory.Exists(datasetDirectory))
        {
            try
            {
                Directory.Delete(datasetDirectory, true);
                _logger.LogInformation($"Deleted dataset {datasetId} from disk.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete dataset {datasetId} from disk.");
            }
        }
    }

    private async Task DeleteFromSqlBlobAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var blob = await _dbContext.DatasetBlobs
            .FirstOrDefaultAsync(b => b.DatasetId == datasetId, cancellationToken);

        if (blob is not null)
        {
            _dbContext.DatasetBlobs.Remove(blob);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Deleted dataset {datasetId} from SQL blob.");
        }
    }
}
