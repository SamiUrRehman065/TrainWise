namespace TrainWise.API.Services.Datasets;

/// <summary>
/// Abstraction for dataset file storage (disk, database, or archive).
/// </summary>
public interface IDatasetStorageService
{
    /// <summary>
    /// Save dataset file to storage (disk or SQL blob).
    /// </summary>
    Task SaveAsync(Guid datasetId, string fileName, Stream fileStream, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieve dataset file from storage.
    /// </summary>
    Task<Stream?> GetAsync(Guid datasetId, CancellationToken cancellationToken);

    /// <summary>
    /// Delete dataset file and metadata from storage.
    /// </summary>
    Task DeleteAsync(Guid datasetId, CancellationToken cancellationToken);

    /// <summary>
    /// Archive old datasets to reduce database/disk footprint.
    /// Returns number of datasets archived.
    /// </summary>
    Task<int> ArchiveOldAsync(int daysOld, CancellationToken cancellationToken);

    /// <summary>
    /// Get storage mode name (e.g., "Disk", "SqlBlob", "Archive").
    /// </summary>
    string GetStorageMode();
}
