namespace TrainWise.Web.Models;

public sealed class TrainResult
{
    public string ExperimentId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = "classification";
    public ClassificationMetrics? ClassificationMetrics { get; set; }
    public RegressionMetrics? RegressionMetrics { get; set; }
    public Dictionary<string, double>? FeatureImportances { get; set; }
    public double TrainingDurationSeconds { get; set; }
    public List<Recommendation> Recommendations { get; set; } = new();
}

public sealed class ClassificationMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public List<List<int>> ConfusionMatrix { get; set; } = new();
    public Dictionary<string, ClassificationReportEntry>? ClassificationReport { get; set; }
}

public sealed class ClassificationReportEntry
{
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public int Support { get; set; }
}

public sealed class RegressionMetrics
{
    public double R2Score { get; set; }
    public double Rmse { get; set; }
    public double Mae { get; set; }
}

public sealed class Recommendation
{
    public string RuleId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
