-- This script backfills FileHash for existing datasets that don't have one.
-- Run this after deploying the FileHash column update.
-- Note: This is a placeholder that documents the need to backfill hashes.
-- In production, you would run a C# utility to compute hashes from the actual files.

-- Mark all datasets without FileHash as needing backfill
-- In a real scenario, a .NET utility would:
-- 1. Query Datasets WHERE FileHash IS NULL
-- 2. Read each file from FilePath
-- 3. Compute SHA256 hash
-- 4. Update the Datasets row with the hash

-- For now, log which datasets need backfill:
-- SELECT DatasetId, UserId, FileName, FilePath FROM Datasets WHERE FileHash IS NULL;

-- A backfill utility should be run separately (see TrainWise.Database/backfill/BackfillFileHashUtility.cs)
