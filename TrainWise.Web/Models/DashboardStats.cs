namespace TrainWise.Web.Models;

/// <summary>
/// Dashboard statistics returned from GET /api/dataset/manage/stats.
/// </summary>
public sealed class DashboardStats
{
    public int TotalDatasets { get; set; }
    public int ActiveDatasets { get; set; }
    public int TotalExperiments { get; set; }
    public double BestAccuracy { get; set; }
    public int ModelsTried { get; set; }
    public long EstimatedSize { get; set; }
    public string StorageMode { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public DateTime MemberSince { get; set; }
}
