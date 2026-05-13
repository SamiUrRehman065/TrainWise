using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainWise.API.Data.Models;

/// <summary>
/// Optional SQL blob storage for dataset files.
/// Allows storing dataset file content directly in the database instead of on disk.
/// Useful for shared environments or cloud deployments without persistent storage.
/// </summary>
public sealed class DatasetBlob
{
    [Key]
    public Guid BlobId { get; set; }

    [Required]
    public Guid DatasetId { get; set; }

    [Required]
    [Column(TypeName = "varbinary(max)")]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; }

    public Dataset? Dataset { get; set; }
}
