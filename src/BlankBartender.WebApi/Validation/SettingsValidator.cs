using BlankBartender.WebApi.Configuration;

namespace BlankBartender.WebApi.Validation;

public class SettingsValidator : ISettingsValidator
{
    public bool IsValid(SettingsValues settings)
    {
        if (settings == null) return false;
        // No specific constraints for now.
        return true;
    }
}
