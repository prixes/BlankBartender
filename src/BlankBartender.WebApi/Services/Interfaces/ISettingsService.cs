using BlankBartender.WebApi.Configuration;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface ISettingsService
    {
        public SettingsValues GetMachineSettings();
        public Task SetMachineSettings(bool useCameraAI, bool useStitter);
        public void AddLiquid(string newLiquid);
        public void RemoveLiquid(string liquidToRemove);
    }
}
