using BlankBartender.UI.Core.Interfaces;

namespace BlankBartender.UI.Mobile.Services;
public class PlatformService : IPlatformService
{
    public string GetPlatformName() => DeviceInfo.Current.Platform.ToString();
}
