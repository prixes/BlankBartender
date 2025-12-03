namespace BlankBartender.WebApi.Configuration;

public static class ConfigurationPaths
{
    public const string ConfigurationDataFolder = "ConfigurationData";
    public const string SettingsFileName = "settings.json";
    public const string LiquidsFileName = "liquids-config.json";
    public const string PumpsFileName = "pump-config.json";

    public static string GetFullPath(string fileName) =>
        Path.Combine(Directory.GetCurrentDirectory(), ConfigurationDataFolder, fileName);
}