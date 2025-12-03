using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlankBartender.UI.Core.Services;

public class StatusService : IStatusService
{
    private HubConnection? hubConnection;
    public bool IsProcessing { get; set; }

    public event Action OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();


    private readonly string url = "";

    public StatusService(NavigationManager NavigationManager)
    {
        var host = new Uri(NavigationManager.BaseUri).Host;
        url = $"http://{host}:5000/ProcessingHub";
    }

    public async Task StartHub()
    {

        if (OperatingSystem.IsBrowser())
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();
        }
        else
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
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
            this.IsProcessing = isProcessing;
            NotifyStateChanged();
        });

        await hubConnection.StartAsync();
    }
}
