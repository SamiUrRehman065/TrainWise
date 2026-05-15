using TrainWise.API.Contracts.Dataset;

namespace TrainWise.API.Services.Datasets;

public interface IDatasetService
{
    Task<DatasetUploadResponse> UploadAsync(IFormFile file, Guid userId, CancellationToken cancellationToken);
    Task<DatasetListDto> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<DatasetSummaryDto?> GetSummaryAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken);
    Task<EdaReportDto?> GetEdaAsync(Guid datasetId, Guid userId, string? targetColumn, CancellationToken cancellationToken);
    Task<DatasetIntelligenceDto?> GetIntelligenceAsync(Guid datasetId, Guid userId, string? targetColumn, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken);
}
