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
    /// Deserialized from MetricsJson for classification tasks.
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

                if (root.TryGetProperty("ClassificationMetrics", out var metricsElement) ||
                    root.TryGetProperty("classificationMetrics", out metricsElement))
                {
                    return JsonSerializer.Deserialize<ClassificationMetrics>(
                        metricsElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return JsonSerializer.Deserialize<ClassificationMetrics>(
                    MetricsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Deserialized from MetricsJson for regression tasks.
    /// </summary>
    public RegressionMetrics? RegressionMetrics
    {
        get
        {
            if (string.IsNullOrWhiteSpace(MetricsJson))
                return null;
            try
            {
                using var doc = JsonDocument.Parse(MetricsJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("RegressionMetrics", out var metricsElement) ||
                    root.TryGetProperty("regressionMetrics", out metricsElement))
                {
                    return JsonSerializer.Deserialize<RegressionMetrics>(
                        metricsElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return JsonSerializer.Deserialize<RegressionMetrics>(
                    MetricsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Deserialized from MetricsJson for visualization.
    /// </summary>
    public ExperimentCharts? Charts
    {
        get
        {
            if (string.IsNullOrWhiteSpace(MetricsJson))
                return null;
            try
            {
                using var doc = JsonDocument.Parse(MetricsJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("Charts", out var chartsElement) ||
                    root.TryGetProperty("charts", out chartsElement))
                {
                    return JsonSerializer.Deserialize<ExperimentCharts>(
                        chartsElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Deserialized from MetricsJson into full TrainResult for easy access.
    /// </summary>
    public TrainResult? Result
    {
        get
        {
            if (string.IsNullOrWhiteSpace(MetricsJson))
                return null;
            try
            {
                return JsonSerializer.Deserialize<TrainResult>(
                    MetricsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }

    // These are populated from the experiment list context, not from the detail endpoint
    public string DatasetName { get; set; } = string.Empty;
}
