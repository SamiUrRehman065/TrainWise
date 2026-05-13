namespace TrainWise.Web.Models;

public sealed class ExperimentList
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<ExperimentListItem> Items { get; set; } = new();
}

public sealed class ExperimentListItem
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
