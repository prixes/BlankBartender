using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Repositories.Interfaces;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("configuration")]
public class PumpsController : ControllerBase
{
    private readonly IPumpsRepository pumpsRepository;
    private readonly IPinService pinService;
    private readonly IStatusService statusService;
    private readonly ILogger<PumpsController> logger;

    public PumpsController(IPumpsRepository pumpsRepository, IPinService pinService, IStatusService statusService, ILogger<PumpsController> logger)
    {
        this.pumpsRepository = pumpsRepository;
        this.pinService = pinService;
        this.statusService = statusService;
        this.logger = logger;
    }

    [HttpGet("liquids/available")]
    public async Task<ActionResult> ReadAvailableLiquidsAsync()
    {
        try
        {
            var liquids = await pumpsRepository.GetAvailableLiquidsAsync();
            return new JsonResult(new { Liquids = liquids ?? Enumerable.Empty<string>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading available liquids");
            return StatusCode(500, new { error = "Failed to read available liquids" });
        }
    }

    [HttpGet("pump")]
    public async Task<ActionResult> ReadCurrentPumpConfigurationAsync()
    {
        try
        {
            var cfg = await pumpsRepository.GetConfigurationAsync();
            return new JsonResult(new { Pumps = cfg.Pumps });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading pump configuration");
            return StatusCode(500, new { error = "Failed to read pump configuration" });
        }
    }

    [HttpPut("pump/change")]
    public async Task<ActionResult> ChangePumpLiquidAsync(int pumpNumber, string liquid)
    {
        try
        {
            await statusService.StartRunning();

            var updated = await pumpsRepository.UpdatePumpLiquidAsync(pumpNumber, liquid);
            if (!updated)
            {
                await statusService.StopRunning();
                return NotFound($"Pump {pumpNumber} not found");
            }

            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing pump liquid");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to change pump liquid" });
        }
    }

    [HttpPost("pumps/all/start")]
    public async Task<ActionResult> StartAllPumpsAsync()
    {
        try
        {
            await statusService.StartRunning();
            var cfg = await pumpsRepository.GetConfigurationAsync();
            foreach (var pump in cfg.Pumps)
            {
                pinService.SwitchPin(pump.Pin, true);
            }
            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting all pumps");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to start pumps" });
        }
    }

    [HttpPost("pump/{pumpNumber}/start")]
    public async Task<ActionResult> StartPumpAsync(int pumpNumber)
    {
        try
        {
            await statusService.StartRunning();
            var cfg = await pumpsRepository.GetConfigurationAsync();
            var pump = cfg.Pumps.FirstOrDefault(x => x.Number == pumpNumber);
            if (pump == null)
            {
                await statusService.StopRunning();
                return NotFound($"Pump {pumpNumber} not found");
            }
            pinService.SwitchPin(pump.Pin, true);
            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting pump");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to start pump" });
        }
    }

    [HttpPost("pumps/all/stop")]
    public async Task<ActionResult> StopAllPumpsAsync()
    {
        try
        {
            await statusService.StartRunning();
            var cfg = await pumpsRepository.GetConfigurationAsync();
            foreach (var pump in cfg.Pumps)
            {
                pinService.SwitchPin(pump.Pin, false);
            }
            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping all pumps");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to stop pumps" });
        }
    }

    [HttpPost("pump/{pumpNumber}/stop")]
    public async Task<ActionResult> StopAsync(int pumpNumber)
    {
        try
        {
            await statusService.StartRunning();
            var cfg = await pumpsRepository.GetConfigurationAsync();
            var pump = cfg.Pumps.FirstOrDefault(x => x.Number == pumpNumber);
            if (pump == null)
            {
                await statusService.StopRunning();
                return NotFound($"Pump {pumpNumber} not found");
            }
            pinService.SwitchPin(pump.Pin, false);
            await statusService.StopRunning();
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping pump");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to stop pump" });
        }
    }
}
