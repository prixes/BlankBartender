namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IStatusService
    {
		Task<bool> IsRunning();
        Task StartRunning();
        Task StopRunning();
        Task SendStatus(bool isProcessing);
    }
}