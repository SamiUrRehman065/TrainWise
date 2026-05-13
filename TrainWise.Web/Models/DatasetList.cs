namespace TrainWise.Web.Models;

public sealed class DatasetList
{
    public List<DatasetListItem> Items { get; set; } = new();
}

public sealed class DatasetListItem
{
    public Guid DatasetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Convenience property mapped from FileName for display purposes.
    /// </summary>
    public string Name => FileName;
}
