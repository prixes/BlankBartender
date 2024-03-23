using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlankBartender.UI.Core.Pages;

public partial class Maintenance
{
    [Inject]
    public IConfigurationService _service { get; set; } = default!;
    public bool isProcessing { get; set; } = false;
    public bool isInitializing { get; set; } = false;
    public IEnumerable<string> liquids;
    public IEnumerable<Pump> pumps;
    public List<bool> pumpsSwitch { get; set; }
    public List<string> selectedLiquids;
    public bool UseCameraAI { get; set; } = false;

    public bool UseStirrer { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        liquids = await _service.GetAllPumpLiquids();
        pumps = await _service.GetPumpConfiguration();
        pumpsSwitch = Enumerable.Repeat(false, pumps.Count()).ToList(); 
        selectedLiquids = pumps.Select(pump => pump.Value).ToList();
        var (cameraAI, stirrer) = await _service.GetSettings();
        UseCameraAI = cameraAI;
        UseStirrer = stirrer;
    }

    protected async Task StartPumps()
    {
        isProcessing = true;
        await _service.StartPumps();
        isProcessing = false;
    }
    protected async Task StopPumps()
    {
        isProcessing = true;
        await _service.StopPumps();
        isProcessing = false;
    }

    protected async Task InitializeLiquidFlow()
    {
        isProcessing = true;
        await _service.InitializeLiquidFlow();
        isProcessing = false;
    }

    protected async Task PumpLiquidChange(int pumpNumber, string liquid)
    {
        isProcessing = true;
        await _service.PumpLiquidChange(pumpNumber, liquid);
        pumps = await _service.GetPumpConfiguration();
        isProcessing = false;
    }

    protected async Task UpdateSettings(bool useCameraAI, bool useStirrer)
    {
        isProcessing = true;
        await _service.SetSettings(useCameraAI, useStirrer);
        isProcessing = false;
    }

    protected async Task PumpStateSwitch(int index)
    {
        isProcessing = true;
        if (pumpsSwitch[index] == false)
        {
            await _service.StartPump(index + 1);
        }
        else
        {
            await _service.StopPump(index + 1);
        }
        isProcessing = false;
        pumpsSwitch[index] = !pumpsSwitch[index];
    }
}
