using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainWise.API.Contracts.Experiments;
using TrainWise.API.Services.Experiments;

namespace TrainWise.API.Controllers.Experiments;

[ApiController]
[Route("api/experiment")]
public sealed class ExperimentController : ControllerBase
{
    private readonly IExperimentService _experimentService;

    public ExperimentController(IExperimentService experimentService)
    {
        _experimentService = experimentService;
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExperimentDetailDto>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _experimentService.GetAsync(id, userId.Value, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}/recommendations")]
    public async Task<ActionResult<List<TrainWise.API.Contracts.Training.RecommendationDto>>> GetRecommendationsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _experimentService.GetRecommendationsAsync(id, userId.Value, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<ExperimentListDto>> GetHistoryAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _experimentService.GetHistoryAsync(userId.Value, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var experiment = await _experimentService.GetAsync(id, userId.Value, cancellationToken);
        if (experiment == null || string.IsNullOrEmpty(experiment.ModelPath))
        {
            return NotFound("Model file not found or not yet generated.");
        }

        var stream = await _experimentService.DownloadModelAsync(experiment.ModelPath, cancellationToken);
        if (stream == null) return NotFound("Model file could not be retrieved from the ML service.");

        return File(stream, "application/octet-stream", $"model_{id}.joblib");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _experimentService.DeleteAsync(id, userId.Value, cancellationToken);
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
