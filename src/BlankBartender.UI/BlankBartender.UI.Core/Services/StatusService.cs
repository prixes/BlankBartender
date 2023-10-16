using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace BlankBartender.UI.Core.Services
{
    public class StatusService : IStatusService
    {
        private HubConnection? hubConnection;
        private readonly IConfiguration Configuration;
        public bool isProcessing { get; set; }

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        public StatusService(IConfiguration Configuration) 
        {
            this.Configuration = Configuration;
        }

        public async Task StartHub()
        {

            if (OperatingSystem.IsBrowser())
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(Configuration.GetValue<string>("ProcessingHub"))
                    .Build();
            }
            else
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(Configuration.GetValue<string>("ProcessingHub"), options =>
                    {
                        options.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                                clientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                            return message;
                        };
                    })
                    .Build();
            }

            hubConnection.On<bool>("SendStatus", (isProcessing) =>
            {
                this.isProcessing = isProcessing;
                NotifyStateChanged();
            });

            await hubConnection.StartAsync();
        }
    }
}
