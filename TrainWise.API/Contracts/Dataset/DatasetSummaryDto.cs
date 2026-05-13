namespace TrainWise.API.Contracts.Dataset;

public sealed class DatasetSummaryDto
{
    public string DatasetId { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public Dictionary<string, string> ColumnTypes { get; set; } = new();
    public Dictionary<string, MissingValueInfoDto> MissingValues { get; set; } = new();
    public Dictionary<string, SummaryStatsDto> SummaryStats { get; set; } = new();
    public Dictionary<string, int>? ClassDistribution { get; set; }
    public List<List<double>> CorrelationMatrix { get; set; } = new();
    public double? ImbalanceRatio { get; set; }
}

public sealed class MissingValueInfoDto
{
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class SummaryStatsDto
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double Std { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}
