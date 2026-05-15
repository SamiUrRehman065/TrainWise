using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainWise.API.Data.Models;

public sealed class Experiment
{
    [Key]
    public Guid ExperimentId { get; set; }

    [Required]
    public Guid DatasetId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string TaskType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string PreprocessingJson { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? HyperparametersJson { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string MetricsJson { get; set; } = string.Empty;

    [Column(TypeName = "float")]
    public double? TrainingDurationSec { get; set; }
    
    public string? ModelPath { get; set; }

    public DateTime CreatedAt { get; set; }

    public Dataset? Dataset { get; set; }
}
