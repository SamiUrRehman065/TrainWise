IF COL_LENGTH('Datasets', 'FileHash') IS NULL
BEGIN
    ALTER TABLE Datasets ADD FileHash NVARCHAR(64) NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Datasets_UserId_FileHash'
      AND object_id = OBJECT_ID('Datasets')
)
BEGIN
    CREATE INDEX IX_Datasets_UserId_FileHash ON Datasets(UserId, FileHash);
END
