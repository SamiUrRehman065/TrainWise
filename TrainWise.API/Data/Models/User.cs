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

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
}
