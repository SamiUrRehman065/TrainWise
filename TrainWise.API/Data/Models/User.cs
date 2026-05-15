using System.ComponentModel.DataAnnotations;

namespace TrainWise.API.Data.Models;

public sealed class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Workspace Preferences
    public string Theme { get; set; } = "dark";
    public string DefaultTaskType { get; set; } = "classification";
    public int AutoArchiveDays { get; set; } = 30;
    public bool StorageOptimization { get; set; } = true;
    public bool IsPremium { get; set; } = false;

    public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
}
