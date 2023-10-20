using BlankBartender.Shared;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IPumpService
    {
        public IEnumerable<Pump> GetConfiguration();
    }
}
