using Microsoft.Extensions.Configuration;

namespace VaultPublisher.Commands;

internal static class ConfigCommandHandler
{
    public static void Get(string? key, IConfiguration config)
    {
        if (string.IsNullOrEmpty(key))
        {
            foreach (var setting in config.AsEnumerable())
            {
                Console.WriteLine($"{setting.Key}: {setting.Value}");
            }
        }
        else
        {
            var value = config[key];
            Console.WriteLine($"{key}: {value}");
        }
    }
    
    public static void Set(string key, string value, IConfiguration config)
    {
        config[key] = value;
        ConfigurationProvider.Save(config);
    }
    
    public static void Remove(string key, IConfiguration config)
    {
        config[key] = null;
        ConfigurationProvider.Save(config);
    }
}