using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainWise.API.Data.Models;

public sealed class Dataset
{
    [Key]
    public Guid DatasetId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? FileHash { get; set; }

    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    public DateTime UploadedAt { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? AnalysisSummaryJson { get; set; }

    public User? User { get; set; }

    public ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public DatasetBlob? Blob { get; set; }
}
