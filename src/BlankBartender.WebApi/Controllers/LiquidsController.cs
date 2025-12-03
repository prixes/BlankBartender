using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Repositories.Interfaces;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("configuration")]
public class LiquidsController : ControllerBase
{
    private readonly ILiquidsRepository _liquidsRepository;
    private readonly ILogger<LiquidsController> _logger;

    public LiquidsController(ILiquidsRepository liquidsRepository, ILogger<LiquidsController> logger)
    {
        _liquidsRepository = liquidsRepository;
        _logger = logger;
    }

    [HttpGet("liquids")]
    public async Task<ActionResult> AllLiquidsAsync()
    {
        try
        {
            var liquids = await _liquidsRepository.GetAllAsync();
            return new JsonResult(new { Liquids = liquids ?? Enumerable.Empty<string>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading liquids configuration");
            return StatusCode(500, new { error = "Failed to read liquids configuration" });
        }
    }

    [HttpPost("liquids/add")]
    public async Task<ActionResult> AddLiquidAsync(string removeLiquid)
    {
        try
        {
            await _liquidsRepository.AddAsync(removeLiquid);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding liquid");
            return StatusCode(500, new { error = "Failed to add liquid" });
        }
    }

    [HttpDelete("liquids/remove")]
    public async Task<ActionResult> RemoveLiquidAsync(string removeLiquid)
    {
        try
        {
            await _liquidsRepository.RemoveAsync(removeLiquid);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing liquid");
            return StatusCode(500, new { error = "Failed to remove liquid" });
        }
    }
}
