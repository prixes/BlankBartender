namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IStirrerService
    {
        public Task StartStirrer();
        public void StopStirrer();
    }
}
