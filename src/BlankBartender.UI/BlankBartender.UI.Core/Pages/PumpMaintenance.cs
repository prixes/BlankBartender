using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlankBartender.UI.Core.Pages;

public partial class PumpMaintenance
{
    [Inject]
    public IConfigurationService _service { get; set; } = default!;
    public bool isProcessing { get; set; } = false;
    public bool isInitializing { get; set; } = false;
    public IEnumerable<string> liquids;
    public IEnumerable<Pump> pumps;
    public List<string> selectedLiquids;

   protected override async Task OnInitializedAsync()
    {
        liquids = await _service.GetAllPumpLiquids();
        pumps = await _service.GetPumpConfiguration();
        selectedLiquids = pumps.Select(pump => pump.Value ).ToList();
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
}
