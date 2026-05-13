namespace TrainWise.API.Contracts.Experiments;

public sealed class ExperimentDetailDto
{
    public Guid ExperimentId { get; set; }
    public Guid DatasetId { get; set; }
    public string DatasetName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string PreprocessingJson { get; set; } = string.Empty;
    public string? HyperparametersJson { get; set; }
    public string MetricsJson { get; set; } = string.Empty;
    public double? TrainingDurationSec { get; set; }
    public DateTime CreatedAt { get; set; }
}
