using BlankBartender.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BlankBartender.WebApi.Notifications
{
    public class ProcessingHub : Hub<IStatusService>
    {
        public async Task SendStatus(bool isProcessing)
            => await Clients.All.SendStatus(isProcessing);

		public override async Task OnConnectedAsync()
		{
			Console.WriteLine($"user with connectionId: {Context.ConnectionId} has connected");
			await base.OnConnectedAsync();
		}
	}
}