namespace TrainWise.API.Contracts.Dataset;

public sealed class DatasetListDto
{
    public List<DatasetListItemDto> Items { get; set; } = new();
}

public sealed class DatasetListItemDto
{
    public Guid DatasetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}
