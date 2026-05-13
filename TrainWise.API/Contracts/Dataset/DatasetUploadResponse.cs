namespace TrainWise.API.Contracts.Dataset;

public sealed class DatasetUploadResponse
{
    public Guid DatasetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public bool IsDuplicate { get; set; }
}
