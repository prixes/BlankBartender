using BlankBartender.UI.Core.Interfaces;

namespace BlankBartender.UI.Web.Services;

public class PlatformService : IPlatformService
{
    public string GetPlatformName() => "Web";
}
