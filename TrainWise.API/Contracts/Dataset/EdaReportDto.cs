namespace TrainWise.API.Contracts.Dataset;

public sealed class EdaReportDto
{
    public string DatasetId { get; set; } = string.Empty;
    public string? TargetColumn { get; set; }
    public List<string> NumericColumns { get; set; } = new();
    public List<string> CategoricalColumns { get; set; } = new();
    public int DuplicateRows { get; set; }
    public double DuplicatePercentage { get; set; }
    public double MemoryUsageMb { get; set; }
    public Dictionary<string, OutlierInfoDto> OutliersByColumn { get; set; } = new();
    public List<FeatureCorrelationDto> TopTargetCorrelations { get; set; } = new();
    public Dictionary<string, int>? TargetDistribution { get; set; }
}

public sealed class OutlierInfoDto
{
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class FeatureCorrelationDto
{
    public string Feature { get; set; } = string.Empty;
    public double Correlation { get; set; }
}
