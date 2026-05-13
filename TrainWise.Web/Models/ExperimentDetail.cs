using System.Text.Json;

namespace TrainWise.Web.Models;

public sealed class ExperimentDetail
{
    public Guid ExperimentId { get; set; }
    public Guid DatasetId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string PreprocessingJson { get; set; } = string.Empty;
    public string? HyperparametersJson { get; set; }
    public string MetricsJson { get; set; } = string.Empty;
    public double? TrainingDurationSec { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Deserialized from HyperparametersJson for easy access.
    /// </summary>
    public Dictionary<string, object>? HyperParameters
    {
        get
        {
            if (string.IsNullOrWhiteSpace(HyperparametersJson))
                return null;
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(HyperparametersJson);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Deserialized from MetricsJson for easy access.
    /// Handles both flat ClassificationMetrics JSON and nested
    /// TrainResultDto format (where metrics are under "ClassificationMetrics" key).
    /// </summary>
    public ClassificationMetrics? Metrics
    {
        get
        {
            if (string.IsNullOrWhiteSpace(MetricsJson))
                return null;
            try
            {
                using var doc = JsonDocument.Parse(MetricsJson);
                var root = doc.RootElement;

                // Try nested ClassificationMetrics first (TrainResultDto format)
                JsonElement metricsElement;
                if (root.TryGetProperty("ClassificationMetrics", out metricsElement) ||
                    root.TryGetProperty("classificationMetrics", out metricsElement))
                {
                    return JsonSerializer.Deserialize<ClassificationMetrics>(
                        metricsElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Fall back to parsing the entire JSON as ClassificationMetrics
                return JsonSerializer.Deserialize<ClassificationMetrics>(
                    MetricsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }
    }

    // These are populated from the experiment list context, not from the detail endpoint
    public string DatasetName { get; set; } = string.Empty;
}
