namespace TrainWise.Web.Models;

public sealed class UserSettingsResponse
{
    public string Username { get; set; } = string.Empty;
    public string Theme { get; set; } = "dark";
    public string DefaultTaskType { get; set; } = "classification";
    public int AutoArchiveDays { get; set; } = 30;
    public bool StorageOptimization { get; set; } = true;
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class UpdateSettingsRequest
{
    public string? Theme { get; set; }
    public string? DefaultTaskType { get; set; }
    public int? AutoArchiveDays { get; set; }
    public bool? StorageOptimization { get; set; }
}
