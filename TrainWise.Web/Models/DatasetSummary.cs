namespace TrainWise.Web.Models;

public sealed class DatasetSummary
{
    public string DatasetId { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public Dictionary<string, string> ColumnTypes { get; set; } = new();
    public Dictionary<string, MissingValueInfo> MissingValues { get; set; } = new();
    public Dictionary<string, SummaryStats> SummaryStats { get; set; } = new();
    public Dictionary<string, int>? ClassDistribution { get; set; }
    public List<List<double>> CorrelationMatrix { get; set; } = new();
    public double? ImbalanceRatio { get; set; }

    // Computed properties for UI convenience
    public List<string> Columns => ColumnTypes.Keys.ToList();
    public List<List<string>> Preview { get; set; } = new();
}

public sealed class MissingValueInfo
{
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class SummaryStats
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double Std { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}
