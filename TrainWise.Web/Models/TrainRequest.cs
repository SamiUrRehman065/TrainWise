namespace TrainWise.Web.Models;

public sealed class TrainRequest
{
    public string DatasetId { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
    public string TaskType { get; set; } = "classification";
    public PreprocessingConfig Preprocessing { get; set; } = new();
    public ModelConfig Model { get; set; } = new();
    public double TrainTestSplit { get; set; } = 0.8;
    public bool CrossValidation { get; set; }
}

public sealed class PreprocessingConfig
{
    public string NullStrategy { get; set; } = "drop";
    public string Encoding { get; set; } = "onehot";
    public string Scaling { get; set; } = "standard";
    public bool ApplySmote { get; set; }
    public List<string> ExcludeColumns { get; set; } = new();
}

public sealed class ModelConfig
{
    public string Name { get; set; } = "LogisticRegression";
    public ModelHyperparameters Hyperparameters { get; set; } = new();
}

public sealed class ModelHyperparameters
{
    public int? NEstimators { get; set; }
    public double? C { get; set; }
    public string? Kernel { get; set; }
    public int? NNeighbors { get; set; }
    public int? MaxIter { get; set; }
}
