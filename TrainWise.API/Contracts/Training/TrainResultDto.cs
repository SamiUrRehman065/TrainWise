namespace TrainWise.API.Contracts.Training;

public sealed class TrainResultDto
{
    public string ExperimentId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = "classification";
    public ClassificationMetricsDto? ClassificationMetrics { get; set; }
    public RegressionMetricsDto? RegressionMetrics { get; set; }
    public Dictionary<string, double>? FeatureImportances { get; set; }
    public double TrainingDurationSeconds { get; set; }
    public List<RecommendationDto> Recommendations { get; set; } = new();
}

public sealed class ClassificationMetricsDto
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public List<List<int>> ConfusionMatrix { get; set; } = new();
}

public sealed class RegressionMetricsDto
{
    public double R2Score { get; set; }
    public double Rmse { get; set; }
    public double Mae { get; set; }
}

public sealed class RecommendationDto
{
    public string RuleId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
