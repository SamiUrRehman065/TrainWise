namespace TrainWise.Web.Components.Common;

public sealed class FilterDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public sealed class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class StorageItem
{
    public string Name { get; set; } = string.Empty;
    public double Size { get; set; }
}

public enum FeedbackType
{
    Success,
    Error,
    Warning,
    Info
}
