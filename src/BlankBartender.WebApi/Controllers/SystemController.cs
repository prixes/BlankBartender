using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Repositories.Interfaces;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("configuration")]
public class SystemController : ControllerBase
{
    private readonly IPumpsRepository pumpsRepository;
    private readonly IPinService pinService;
    private readonly IDisplayService displayService;
    private readonly IStatusService statusService;
    private readonly ILogger<SystemController> logger;

    public SystemController(IPumpsRepository pumpsRepository, IPinService pinService, IDisplayService displayService, IStatusService statusService, ILogger<SystemController> logger)
    {
        this.pumpsRepository = pumpsRepository;
        this.pinService = pinService;
        this.displayService = displayService;
        this.statusService = statusService;
        this.logger = logger;
    }

    [HttpPost("initialize")]
    public async Task<ActionResult> InitializeLiquidFlowAsync()
    {
        try
        {
            await statusService.StartRunning();

            var cfg = await pumpsRepository.GetConfigurationAsync();
            var pumps = cfg.Pumps;

            int[] countdownTimes = new int[] { 6, 10, 11, 6, 7, 8, 9, 10, 13, 14, 11, 12, 12 };
            int count = Math.Min(pumps.Count, countdownTimes.Length);
            int maxCountdownTime = countdownTimes.Take(count).DefaultIfEmpty(0).Max();

            foreach (var pump in pumps)
            {
                pinService.SwitchPin(pump.Pin, true);
            }

            await displayService.WriteFirstLineDisplay("System fill");

            var pumpOffTasks = countdownTimes
                .Take(count)
                .Select((time, index) => TurnOffPumpAfterDelayAsync(pumps[index].Pin, time))
                .ToList();

            var displayCountdownTask = RunCountdownDisplayAsync(maxCountdownTime);

            await Task.WhenAll(pumpOffTasks.Concat(new[] { displayCountdownTask }));

            displayService.MachineReadyForUse();
            await statusService.StopRunning();

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing liquid flow");
            await statusService.StopRunning();
            return StatusCode(500, new { error = "Failed to initialize liquid flow" });
        }
    }

    private async Task RunCountdownDisplayAsync(int seconds)
    {
        for (int sec = seconds; sec > 0; sec--)
        {
            await displayService.WriteSecondLineDisplay($"Seconds {sec} left");
            await Task.Delay(1000);
        }
    }

    private async Task TurnOffPumpAfterDelayAsync(int pin, int delaySeconds)
    {
        await Task.Delay(delaySeconds * 1000);
        pinService.SwitchPin(pin, false);
    }
}
