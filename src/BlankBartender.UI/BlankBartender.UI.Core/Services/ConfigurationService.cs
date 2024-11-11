using BlankBartender.Shared;
using BlankBartender.UI.Core.Helpers;
using BlankBartender.UI.Core.Interfaces;
using System.Text.Json;

namespace BlankBartender.UI.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationClient _configurationClient;

        public ConfigurationService(IConfigurationClient configurationClient)
		{
            _configurationClient = configurationClient;
        }

        public async Task<bool> StartPumps()
        {
            var response = await _configurationClient.StartAllPumpsAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }
        public async Task<bool> StartPump(int pumpNumber)
        {
            var response = await _configurationClient.StartPumpAsync(pumpNumber);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<bool> StopPumps()
        {
            var response = await _configurationClient.StopAllPumpsAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<bool> StopPump(int pumpNumber)
        {
            var response = await _configurationClient.StopAsync(pumpNumber);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<bool> InitializeLiquidFlow()
        {
            var response = await _configurationClient.InitializeLiquidFlowAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<IEnumerable<string>> GetAllLiquids()
        {
            var response = await _configurationClient.AllLiquidsAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return await RequestHandler.ParseResponseJsonAsync<IEnumerable<string>>(response, "liquids");
        }

        public async Task<IEnumerable<string>> GetReadAvailableLiquids()
        {
            var response = await _configurationClient.ReadAvailableLiquidsAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return await RequestHandler.ParseResponseJsonAsync<IEnumerable<string>>(response, "liquids");
        }

        public async Task AddLiquid(string liquidName)
        {
            var response = await _configurationClient.AddLiquidAsync(liquidName);
            await RequestHandler.ValidateResponseAsync(response);
            return;
        }

        public async Task RemoveLiquid(string liquidName)
        {
            var response = await _configurationClient.RemoveLiquidAsync(liquidName);
            await RequestHandler.ValidateResponseAsync(response);
            return;
        }

        public async Task<IEnumerable<Pump>> GetPumpConfiguration()
        {
            var response = await _configurationClient.ReadCurrentPumpConfigurationAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return await RequestHandler.ParseResponseJsonAsync<IEnumerable<Pump>>(response, "pumps");
        }

        public async Task<bool> PumpLiquidChange(int pumpNumber, string liquid)
        {
            var response = await _configurationClient.ChangePumpLiquidAsync(pumpNumber, liquid);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<(bool, bool)> GetSettings()
        {
            var response = await _configurationClient.GetMachineSettingsAsync();

            using var jsonDocument = await JsonDocument.ParseAsync(response.Stream);

            var root = jsonDocument.RootElement;

            var useCameraAI = root.GetProperty("useCameraAI").GetBoolean();
            var useStirrer = root.GetProperty("useStirrer").GetBoolean();

            return (useCameraAI, useStirrer);
        }

        public async Task<bool> SetSettings(bool UseCameraAI, bool UseStirrer)
        {
            var response = await _configurationClient.SetMachineSettingsAsync(UseCameraAI, UseStirrer);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }
    }
}
