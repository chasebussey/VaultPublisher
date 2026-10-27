using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace VaultPublisher.Commands;

internal static class ConfigCommand
{
    public static Command Create() => Create(ConfigurationProvider.GetConfiguration);

    internal static Command Create(Func<IConfiguration> getConfiguration)
    {
        var command = new Command("config", "Manage configuration settings");

        command.Subcommands.Add(BuildListCommand(getConfiguration));
        command.Subcommands.Add(BuildGetCommand(getConfiguration));
        command.Subcommands.Add(BuildSetCommand(getConfiguration));
        command.Subcommands.Add(BuildRemoveCommand(getConfiguration));

        return command;
    }

    private static Command BuildListCommand(Func<IConfiguration> getConfiguration)
    {
        var command = new Command("list", "List all configuration settings");
        command.SetAction(_ => ConfigCommandHandler.List(getConfiguration()));

        return command;
    }

    private static Command BuildGetCommand(Func<IConfiguration> getConfiguration)
    {
        var command = new Command("get", "Get a configuration setting by key.");
        var keyArg = new Argument<string>("key")
        {
            Description = "The key of the configuration setting"
        };
        command.Arguments.Add(keyArg);
        command.SetAction(parseResult => ConfigCommandHandler.Get(parseResult.GetRequiredValue(keyArg), getConfiguration()));

        return command;
    }

    private static Command BuildSetCommand(Func<IConfiguration> getConfiguration)
    {
        var command = new Command("set", "Sets the configuration setting to the provided value.");
        var keyArg = new Argument<string>("key")
        {
            Description = "The key of the configuration setting to set"
        };
        var valArg = new Argument<string>("value")
        {
            Description = "The value of the configuration setting to set"
        };
        command.Arguments.Add(keyArg);
        command.Arguments.Add(valArg);
        command.SetAction(parseResult => ConfigCommandHandler.Set(
            parseResult.GetRequiredValue(keyArg), parseResult.GetRequiredValue(valArg), getConfiguration()));

        return command;
    }

    private static Command BuildRemoveCommand(Func<IConfiguration> getConfiguration)
    {
        var command = new Command("remove", "Removes the configuration setting with the provided key.");
        var keyArg = new Argument<string>("key")
        {
            Description = "The key of the configuration setting to remove"
        };
        command.Arguments.Add(keyArg);
        command.SetAction(parseResult => ConfigCommandHandler.Remove(parseResult.GetRequiredValue(keyArg), getConfiguration()));

        return command;
    }
}
