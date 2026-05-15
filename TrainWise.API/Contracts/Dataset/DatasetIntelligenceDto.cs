using System.Collections.Generic;

namespace TrainWise.API.Contracts.Dataset;

/// <summary>
/// Professional-grade dataset intelligence and profiling report.
/// Includes data quality scores, statistical deep-dives, and ML readiness insights.
/// </summary>
public sealed class DatasetIntelligenceDto
{
    public string DatasetId { get; set; } = string.Empty;
    public DatasetOverviewDto Overview { get; set; } = new();
    public List<FeatureProfileDto> Features { get; set; } = new();
    public CorrelationAnalysisDto Correlations { get; set; } = new();
    public MissingValueAnalysisDto MissingValues { get; set; } = new();
    public TargetAnalysisDto? TargetAnalysis { get; set; }
    public DataQualityReportDto QualityReport { get; set; } = new();
    public MlReadinessReportDto MlReadiness { get; set; } = new();
}

public sealed class DatasetOverviewDto
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public long SizeBytes { get; set; }
    public int DuplicateRows { get; set; }
    public double DuplicatePercentage { get; set; }
    public int TotalCells { get; set; }
    public int MissingCells { get; set; }
    public double MissingPercentage { get; set; }
    public Dictionary<string, int> TypeDistribution { get; set; } = new(); // Numerical, Categorical, Datetime, etc.
    public string TargetColumn { get; set; } = string.Empty;
    public string ProblemType { get; set; } = "Unknown"; // Binary, Multi-class, Regression
    public double SparsityScore { get; set; }
}

public sealed class FeatureProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsTarget { get; set; }
    public bool IsCandidateId { get; set; }
    public bool IsConstant { get; set; }
    public double RelevanceScore { get; set; }
    
    // Statistics
    public Dictionary<string, double> Stats { get; set; } = new(); // Mean, Median, Skewness, etc.
    public int UniqueCount { get; set; }
    public double UniqueRatio { get; set; }
    
    // Quality
    public int MissingCount { get; set; }
    public double MissingPercentage { get; set; }
    public int OutlierCount { get; set; }
    public double OutlierPercentage { get; set; }
    
    // Distribution for charts
    public List<double>? HistogramValues { get; set; }
    public List<string>? HistogramLabels { get; set; }
    public List<CategoryFreqDto>? TopCategories { get; set; }
    
    // Insights
    public List<string> Alerts { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public sealed class CategoryFreqDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class CorrelationAnalysisDto
{
    public List<string> Columns { get; set; } = new();
    public List<List<double>> Matrix { get; set; } = new();
    public List<FeatureCorrelationDto> HighCorrelations { get; set; } = new();
}

public sealed class FeatureCorrelationDto
{
    public string Feature { get; set; } = string.Empty;
    public double Correlation { get; set; }
}

public sealed class MissingValueAnalysisDto
{
    public Dictionary<string, int> MissingByColumn { get; set; } = new();
    public List<string> Patterns { get; set; } = new(); // MCAR, etc.
    public List<string> Recommendations { get; set; } = new();
}

public sealed class TargetAnalysisDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, double> Stats { get; set; } = new();
    public Dictionary<string, int>? Distribution { get; set; }
    public List<string> Insights { get; set; } = new();
}

public sealed class DataQualityReportDto
{
    public double OverallScore { get; set; } // 0-100
    public string RiskLevel { get; set; } = "Low";
    public List<DataQualityIssueDto> Issues { get; set; } = new();
}

public sealed class DataQualityIssueDto
{
    public string Severity { get; set; } = "Info";
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Column { get; set; }
}

public sealed class MlReadinessReportDto
{
    public bool IsReady { get; set; }
    public List<string> RequiredPreprocessing { get; set; } = new();
    public List<string> SuitableModels { get; set; } = new();
    public List<string> ActionableSteps { get; set; } = new();
}
