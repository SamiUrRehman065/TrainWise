using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainWise.API.Contracts.Training;
using TrainWise.API.Services.Training;

namespace TrainWise.API.Controllers.Training;

[ApiController]
[Route("api/train")]
public sealed class TrainController : ControllerBase
{
    private readonly ITrainingService _trainingService;

    public TrainController(ITrainingService trainingService)
    {
        _trainingService = trainingService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TrainResultDto>> TrainAsync(
        [FromBody] TrainRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _trainingService.TrainAsync(request, userId.Value, cancellationToken);
        if (result is null)
        {
            return BadRequest();
        }

        return Ok(result);
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
