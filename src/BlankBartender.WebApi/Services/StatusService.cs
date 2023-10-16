using BlankBartender.WebApi.Notifications;
using BlankBartender.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BlankBartender.WebApi.Services
{
    public class StatusService : IStatusService
    {
       private readonly IHubContext<ProcessingHub, IStatusService> _statusHubHub;

        public StatusService(IHubContext<ProcessingHub, IStatusService> statusHubHub)
        {
            _statusHubHub = statusHubHub;
        }
        private bool _isProcessing = false;
        private bool IsProcesing
        {
            get
            {
                return _isProcessing;
            }
            set
            {
                _isProcessing = value;
                _statusHubHub.Clients.All.SendStatus(value);
            }
        }

        public async Task<bool> IsRunning() => await Task.FromResult(this.IsProcesing);
        public async Task SendStatus(bool isProcessing) => this.IsProcesing = isProcessing;
        public async Task StartRunning()
        { 
            this.IsProcesing = true;
        }
        public async Task StopRunning()
        {
            this.IsProcesing = false;
        }
    }
}