using Microsoft.EntityFrameworkCore;
using TrainWise.API.Data.Models;

namespace TrainWise.API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Dataset> Datasets => Set<Dataset>();
    public DbSet<Experiment> Experiments => Set<Experiment>();
    public DbSet<DatasetBlob> DatasetBlobs => Set<DatasetBlob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.UserId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<Dataset>(entity =>
        {
            entity.HasKey(e => e.DatasetId);
            entity.Property(e => e.DatasetId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(512).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(64);
            entity.Property(e => e.AnalysisSummaryJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => new { e.UserId, e.FileHash });
            entity.HasOne(e => e.User)
                .WithMany(u => u.Datasets)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Experiment>(entity =>
        {
            entity.HasKey(e => e.ExperimentId);
            entity.Property(e => e.ExperimentId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ModelName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TaskType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PreprocessingJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.HyperparametersJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.MetricsJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.TrainingDurationSec).HasColumnType("float");
            entity.HasOne(e => e.Dataset)
                .WithMany(d => d.Experiments)
                .HasForeignKey(e => e.DatasetId);
        });

        modelBuilder.Entity<DatasetBlob>(entity =>
        {
            entity.HasKey(e => e.BlobId);
            entity.Property(e => e.BlobId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FileContent).HasColumnType("varbinary(max)").IsRequired();
            entity.HasOne(e => e.Dataset)
                .WithOne(d => d.Blob)
                .HasForeignKey<DatasetBlob>(e => e.DatasetId);
        });
    }
}
