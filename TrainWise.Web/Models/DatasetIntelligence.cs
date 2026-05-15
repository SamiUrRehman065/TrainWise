using System.Collections.Generic;

namespace TrainWise.Web.Models;

public sealed class DatasetIntelligence
{
    public string DatasetId { get; set; } = string.Empty;
    public DatasetOverview Overview { get; set; } = new();
    public List<FeatureProfile> Features { get; set; } = new();
    public CorrelationAnalysis Correlations { get; set; } = new();
    public MissingValueAnalysis MissingValues { get; set; } = new();
    public TargetAnalysis? TargetAnalysis { get; set; }
    public DataQualityReport QualityReport { get; set; } = new();
    public MlReadinessReport MlReadiness { get; set; } = new();
}

public sealed class DatasetOverview
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public long SizeBytes { get; set; }
    public int DuplicateRows { get; set; }
    public double DuplicatePercentage { get; set; }
    public int TotalCells { get; set; }
    public int MissingCells { get; set; }
    public double MissingPercentage { get; set; }
    public Dictionary<string, int> TypeDistribution { get; set; } = new();
    public string TargetColumn { get; set; } = string.Empty;
    public string ProblemType { get; set; } = "Unknown";
    public double SparsityScore { get; set; }
}

public sealed class FeatureProfile
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsTarget { get; set; }
    public bool IsCandidateId { get; set; }
    public bool IsConstant { get; set; }
    public double RelevanceScore { get; set; }
    public Dictionary<string, double> Stats { get; set; } = new();
    public int UniqueCount { get; set; }
    public double UniqueRatio { get; set; }
    public int MissingCount { get; set; }
    public double MissingPercentage { get; set; }
    public int OutlierCount { get; set; }
    public double OutlierPercentage { get; set; }
    public List<double>? HistogramValues { get; set; }
    public List<string>? HistogramLabels { get; set; }
    public List<CategoryFreq>? TopCategories { get; set; }
    public List<string> Alerts { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public sealed class CategoryFreq
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class CorrelationAnalysis
{
    public List<string> Columns { get; set; } = new();
    public List<List<double>> Matrix { get; set; } = new();
    public List<FeatureCorrelation> HighCorrelations { get; set; } = new();
}

public sealed class MissingValueAnalysis
{
    public Dictionary<string, int> MissingByColumn { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public sealed class TargetAnalysis
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, double> Stats { get; set; } = new();
    public Dictionary<string, int>? Distribution { get; set; }
    public List<string> Insights { get; set; } = new();
}

public sealed class DataQualityReport
{
    public double OverallScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<DataQualityIssue> Issues { get; set; } = new();
}

public sealed class DataQualityIssue
{
    public string Severity { get; set; } = "Info";
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Column { get; set; }
}

public sealed class MlReadinessReport
{
    public bool IsReady { get; set; }
    public List<string> RequiredPreprocessing { get; set; } = new();
    public List<string> SuitableModels { get; set; } = new();
    public List<string> ActionableSteps { get; set; } = new();
}
