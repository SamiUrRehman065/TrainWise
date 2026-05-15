using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainWise.API.Contracts.User;
using TrainWise.API.Data;

namespace TrainWise.API.Controllers.User;

[Authorize]
[ApiController]
[Route("api/user")]
public sealed class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (user == null) return NotFound();

        return Ok(new UserSettingsResponse
        {
            Username = user.Username,
            Theme = user.Theme,
            DefaultTaskType = user.DefaultTaskType,
            AutoArchiveDays = user.AutoArchiveDays,
            StorageOptimization = user.StorageOptimization,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest request)
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (user == null) return NotFound();

        if (request.Theme != null) user.Theme = request.Theme;
        if (request.DefaultTaskType != null) user.DefaultTaskType = request.DefaultTaskType;
        if (request.AutoArchiveDays.HasValue) user.AutoArchiveDays = request.AutoArchiveDays.Value;
        if (request.StorageOptimization.HasValue) user.StorageOptimization = request.StorageOptimization.Value;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Settings updated successfully" });
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate()
    {
        // For now, just a placeholder for the UI feature
        return Ok(new { message = "Workspace deactivation requested. Please contact support to finalize." });
    }

    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }
}
