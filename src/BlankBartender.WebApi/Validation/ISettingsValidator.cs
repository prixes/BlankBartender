using BlankBartender.WebApi.Configuration;

namespace BlankBartender.WebApi.Validation;

public interface ISettingsValidator
{
    bool IsValid(SettingsValues settings);
}
