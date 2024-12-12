using System.CommandLine.Binding;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration; 

namespace VaultPublisher;

public class ConfigurationProvider : BinderBase<IConfiguration>
{
    private static readonly string _appName = "quartzpublisher";

    private static string ConfigurationDirectory => Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile), $".{_appName}");

    private static string ConfigurationFile => Path.Combine(ConfigurationDirectory, "config.json");
    
    protected override IConfiguration GetBoundValue(BindingContext bindingContext) => GetConfiguration();

    public static IConfiguration GetConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(ConfigurationFile, optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }
    
    public static bool GetBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
    
    public static DirectoryInfo? GetDirectory(string? path) => string.IsNullOrEmpty(path) ? null : new DirectoryInfo(path);

    public static void Save(IConfiguration config)
    {
        if (!Directory.Exists(ConfigurationDirectory)) Directory.CreateDirectory(ConfigurationDirectory);
        
        var flatConfig = config.AsEnumerable()
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);
        
        var json = JsonSerializer.Serialize(flatConfig, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        File.WriteAllText(ConfigurationFile, json);
    }
}