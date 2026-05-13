namespace TrainWise.Web.Models;

/// <summary>
/// Response from GET /api/dataset/manage/training-history.
/// </summary>
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
