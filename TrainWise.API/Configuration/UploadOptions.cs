namespace TrainWise.API.Configuration;

public sealed class UploadOptions
{
    public int MaxFileSizeMb { get; set; } = 50;
    public string StoragePath { get; set; } = "/data/uploads";
}
