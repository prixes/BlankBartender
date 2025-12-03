using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("configuration")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService settingsService;
    private readonly IStatusService statusService;
    private readonly ILogger<SettingsController> logger;

    public SettingsController(ISettingsService settingsService, IStatusService statusService, ILogger<SettingsController> logger)
    {
        this.settingsService = settingsService;
        this.statusService = statusService;
        this.logger = logger;
    }

    [HttpGet("settings")]
    public async Task<ActionResult> GetMachineSettings()
    {
        try
        {
            var settingsValues = await settingsService.GetMachineSettingsAsync();
            return new JsonResult(new
            {
                settingsValues.UseCameraAI,
                settingsValues.UseStirrer,
                settingsValues.UseIceDispenser
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting machine settings");
            return StatusCode(500, new { error = "Failed to get settings" });
        }
    }

    [HttpPut("settings")]
    public async Task<ActionResult> SetMachineSettingsAsync(bool useCameraAI, bool useStitter, bool useIceDispenser)
    {
        try
        {
            await statusService.StartRunning();
            await settingsService.SetMachineSettingsAsync(useCameraAI, useStitter, useIceDispenser);
            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting machine settings");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to set settings" });
        }
    }
}
