namespace TrainWise.API.Services.Datasets;

/// <summary>
/// Configuration for dataset storage backend.
/// </summary>
public sealed class DatasetStorageOptions
{
    /// <summary>
    /// Storage mode: "Disk" (default) or "SqlBlob" (database).
    /// </summary>
    public string Mode { get; set; } = "Disk";

    /// <summary>
    /// Enable automatic archival of old datasets (days before archival).
    /// Set to 0 to disable auto-archive.
    /// </summary>
    public int AutoArchiveAfterDays { get; set; } = 0;

    /// <summary>
    /// Path to archive storage (for archived datasets).
    /// Only used if AutoArchiveAfterDays > 0.
    /// </summary>
    public string? ArchivePath { get; set; }
}
