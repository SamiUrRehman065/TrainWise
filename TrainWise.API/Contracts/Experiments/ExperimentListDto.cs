namespace TrainWise.API.Contracts.Experiments;

public sealed class ExperimentListDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<ExperimentListItemDto> Items { get; set; } = new();
}

public sealed class ExperimentListItemDto
{
    public Guid ExperimentId { get; set; }
    public Guid DatasetId { get; set; }
    public string DatasetName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string MetricsSummary { get; set; } = string.Empty;
    public string PreprocessingSummary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
