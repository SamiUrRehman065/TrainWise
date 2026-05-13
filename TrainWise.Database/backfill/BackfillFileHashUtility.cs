using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TrainWise.API.Data;

namespace TrainWise.Database.Backfill;

/// <summary>
/// Utility to backfill FileHash for existing datasets that were uploaded before hash column was added.
/// Run this after deploying the FileHash column update.
/// 
/// Usage:
///   dotnet run --project TrainWise.API -- backfill-file-hash
/// </summary>
public sealed class BackfillFileHashUtility
{
    private readonly AppDbContext _dbContext;

    public BackfillFileHashUtility(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🔄 Starting FileHash backfill...");

        var datasetsNeedingHash = await _dbContext.Datasets
            .Where(d => d.FileHash == null)
            .OrderBy(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

        if (datasetsNeedingHash.Count == 0)
        {
            Console.WriteLine("✅ All datasets already have FileHash.");
            return;
        }

        Console.WriteLine($"📋 Found {datasetsNeedingHash.Count} datasets needing hash backfill.");

        var updated = 0;
        var errors = 0;

        foreach (var dataset in datasetsNeedingHash)
        {
            try
            {
                if (!File.Exists(dataset.FilePath))
                {
                    Console.WriteLine($"⚠️  File not found: {dataset.FilePath} (DatasetId: {dataset.DatasetId})");
                    errors++;
                    continue;
                }

                var fileHash = await ComputeFileHashAsync(dataset.FilePath, cancellationToken);
                dataset.FileHash = fileHash;
                updated++;

                Console.WriteLine($"✅ Hashed: {dataset.FileName} → {fileHash[..8]}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error hashing {dataset.FileName}: {ex.Message}");
                errors++;
            }
        }

        if (updated > 0)
        {
            Console.WriteLine($"\n💾 Saving {updated} updated datasets...");
            await _dbContext.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"✅ Backfill complete: {updated} datasets updated, {errors} errors.");
        }
        else
        {
            Console.WriteLine($"⚠️  No datasets were updated. ({errors} errors)");
        }
    }

    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
