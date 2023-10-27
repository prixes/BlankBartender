namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IDetectionService
    {
        public Task<bool> DetectGlass();
    }
}
