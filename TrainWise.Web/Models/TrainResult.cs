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
    public ExperimentCharts? Charts { get; set; }
}

public sealed class ExperimentCharts
{
    public ConfusionMatrixData? ConfusionMatrix { get; set; }
    public RocCurveData? RocCurve { get; set; }
    public PrecisionRecallData? PrecisionRecall { get; set; }
    public FeatureImportanceData? FeatureImportance { get; set; }
    public ActualVsPredictedData? ActualVsPredicted { get; set; }
    public ResidualsScatterData? ResidualsScatter { get; set; }
    public ResidualsDistributionData? ResidualsDistribution { get; set; }
    public ProbabilityDistData? ProbabilityDist { get; set; }
    public QqPlotData? QqPlot { get; set; }
    public ClassComparisonData? ClassComparison { get; set; }
    public CrossValidationData? CrossValidation { get; set; }
}

public sealed class ProbabilityDistData
{
    public List<double> Confidences { get; set; } = new();
    public int Bins { get; set; }
}

public sealed class QqPlotData
{
    public List<double> Theoretical { get; set; } = new();
    public List<double> Sample { get; set; } = new();
    public List<double> Line { get; set; } = new();
}

public sealed class ClassComparisonData
{
    public List<string> Labels { get; set; } = new();
    public List<int> ActualCounts { get; set; } = new();
    public List<int> PredictedCounts { get; set; } = new();
}

public sealed class CrossValidationData
{
    public List<string> Folds { get; set; } = new();
    public List<double> Scores { get; set; } = new();
    public double Mean { get; set; }
}

public sealed class ConfusionMatrixData
{
    public List<List<int>> Matrix { get; set; } = new();
    public List<string> XLabels { get; set; } = new();
    public List<string> YLabels { get; set; } = new();
}

public sealed class RocCurveData
{
    public List<double> Fpr { get; set; } = new();
    public List<double> Tpr { get; set; } = new();
    public double Auc { get; set; }
    public List<double> Baseline { get; set; } = new();
}

public sealed class PrecisionRecallData
{
    public List<double> Precision { get; set; } = new();
    public List<double> Recall { get; set; } = new();
    public double AvgPrecision { get; set; }
}

public sealed class FeatureImportanceData
{
    public List<string> Names { get; set; } = new();
    public List<double> Values { get; set; } = new();
}

public sealed class ActualVsPredictedData
{
    public List<double> Actual { get; set; } = new();
    public List<double> Predicted { get; set; } = new();
    public List<double> Reference { get; set; } = new();
}

public sealed class ResidualsScatterData
{
    public List<double> Predicted { get; set; } = new();
    public List<double> Residuals { get; set; } = new();
    public List<double> ZeroLine { get; set; } = new();
}

public sealed class ResidualsDistributionData
{
    public List<double> Residuals { get; set; } = new();
    public double Mean { get; set; }
    public double Std { get; set; }
}

public sealed class ClassificationMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public List<List<int>> ConfusionMatrix { get; set; } = new();
    public List<string>? ClassLabels { get; set; }
    public Dictionary<string, ClassificationReportEntry>? ClassificationReport { get; set; }
    public double? TrainAccuracy { get; set; }
    public double? TrainF1Score { get; set; }
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
    public double? Mse { get; set; }
    public double? TrainR2 { get; set; }
    public double? TrainRmse { get; set; }
}

public sealed class Recommendation
{
    public string RuleId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
}
