using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace VaultPublisher.Commands;

internal static class ConfigCommand
{
    public static Command Create()
    {
        var command = new Command("config", "Manage configuration settings");

        command.AddCommand(BuildListCommand());
        command.AddCommand(BuildGetCommand());
        command.AddCommand(BuildSetCommand());
        command.AddCommand(BuildRemoveCommand());

        return command;
    }

    private static Command BuildListCommand()
    {
        var command = new Command("list", "List all configuration settings");
        
        command.SetHandler(ConfigCommandHandler.List, new ConfigurationProvider());
        
        return command;
    }
    
    private static Command BuildGetCommand()
    {
        var command = new Command("get", "Get a configuration setting by key.");

        var keyArg = new Argument<string>(name: "key", description: "The key of the configuration setting");
        command.AddArgument(keyArg);
        
        command.SetHandler(ConfigCommandHandler.Get, keyArg, new ConfigurationProvider());

        return command;
    }

    private static Command BuildSetCommand()
    {
        var command = new Command("set", "Sets the configuration setting to the provided value.");
        
        var keyArg = new Argument<string>(name: "key", description: "The key of the configuration setting to set");
        var valArg = new Argument<string>(name: "value", description: "The value of the configuration setting to set");
        
        command.AddArgument(keyArg);
        command.AddArgument(valArg);
        
        command.SetHandler(ConfigCommandHandler.Set, keyArg, valArg, new ConfigurationProvider());

        return command;
    }
    
    private static Command BuildRemoveCommand()
    {
        var command = new Command("remove", "Removes the configuration setting with the provided key.");
        
        var keyArg = new Argument<string>("key", "The key of the configuration setting to remove");
        command.AddArgument(keyArg);
        
        command.SetHandler(ConfigCommandHandler.Remove, keyArg, new ConfigurationProvider());

        return command;
    }
}