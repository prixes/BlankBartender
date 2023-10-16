

namespace BlankBartender.UI.Core.Interfaces
{
    public interface IStatusService
    {
        public bool isProcessing { get; set; }
        public event Action OnChange;

        public Task StartHub();
    }
}
