-- Add SQL blob storage table for optional database-backed dataset storage.
-- This allows storing dataset file content directly in the database.
-- Use this when you need shared storage without persistent disk access.

CREATE TABLE DatasetBlobs (
    BlobId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DatasetBlobs PRIMARY KEY DEFAULT NEWID(),
    DatasetId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    FileContent VARBINARY(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_DatasetBlobs_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT FK_DatasetBlobs_Datasets_DatasetId FOREIGN KEY (DatasetId)
        REFERENCES Datasets (DatasetId) ON DELETE CASCADE
);

CREATE INDEX IX_DatasetBlobs_DatasetId ON DatasetBlobs(DatasetId);
