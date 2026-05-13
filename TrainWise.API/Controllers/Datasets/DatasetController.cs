using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainWise.API.Contracts.Dataset;
using TrainWise.API.Services.Datasets;

namespace TrainWise.API.Controllers.Datasets;

[ApiController]
[Route("api/dataset")]
public sealed class DatasetController : ControllerBase
{
    private readonly IDatasetService _datasetService;

    public DatasetController(IDatasetService datasetService)
    {
        _datasetService = datasetService;
    }

    [Authorize]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DatasetUploadResponse>> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _datasetService.UploadAsync(file, userId.Value, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<DatasetListDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _datasetService.GetAllAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<DatasetSummaryDto>> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var summary = await _datasetService.GetSummaryAsync(id, userId.Value, cancellationToken);
        if (summary is null)
        {
            return NotFound();
        }

        return Ok(summary);
    }

    [Authorize]
    [HttpGet("{id:guid}/eda")]
    public async Task<ActionResult<EdaReportDto>> GetEdaAsync(
        Guid id,
        [FromQuery] string? targetColumn,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var eda = await _datasetService.GetEdaAsync(id, userId.Value, targetColumn, cancellationToken);
        if (eda is null)
        {
            return NotFound();
        }

        return Ok(eda);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _datasetService.DeleteAsync(id, userId.Value, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok(new { success = true });
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null)
        {
            return null;
        }

        return Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
