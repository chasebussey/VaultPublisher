using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace VaultPublisher.Commands;

public static class PublishCommand
{
    public static Command Create() => Create(ConfigurationProvider.GetConfiguration());

    internal static Command Create(IConfiguration config)
    {
        var command = new Command("publish", "Publishes the contents of the source directory to the destination directory");

        var sourceDirectoryOption = new Option<DirectoryInfo?>("--source", "-s")
        {
            Description = "Source directory for files to publish, e.g. /path/to/vault",
            DefaultValueFactory = _ => ConfigurationProvider.GetDirectory(config["source"])
        };
        var destinationDirectoryOption = new Option<DirectoryInfo?>("--destination", "-d")
        {
            Description = "Destination directory for files to publish, e.g. /path/to/content",
            DefaultValueFactory = _ => ConfigurationProvider.GetDirectory(config["destination"])
        };
        var verboseOption = new Option<bool>("--verbose", "-v")
        {
            Description = "Enable verbose output"
        };
        var noDeleteOption = new Option<bool>("--no-delete")
        {
            Description = "Disable deletion of files not marked for publishing",
            DefaultValueFactory = _ => ConfigurationProvider.GetBool(config["noDelete"], defaultValue: false)
        };
        var showPublishedOption = new Option<bool>("--show-published")
        {
            Description = "Show files that were published"
        };
        var previewOption = new Option<bool>("--preview")
        {
            Description = "Preview files that would be published and/or deleted without actually publishing them"
        };
        var excludeDirsOption = new Option<string[]>("--exclude-dirs")
        {
            Description = "Directory names to exclude from publishing at any depth",
            DefaultValueFactory = _ => ConfigurationProvider.GetArray(config["excludeDirs"])
        };

        command.Options.Add(sourceDirectoryOption);
        command.Options.Add(destinationDirectoryOption);
        command.Options.Add(verboseOption);
        command.Options.Add(noDeleteOption);
        command.Options.Add(showPublishedOption);
        command.Options.Add(previewOption);
        command.Options.Add(excludeDirsOption);

        command.SetAction(parseResult =>
        {
            var source = parseResult.GetValue(sourceDirectoryOption);
            var destination = parseResult.GetValue(destinationDirectoryOption);
            if (source is null || destination is null)
            {
                Console.Error.WriteLine("Source and destination directories must be provided.");
                return 1;
            }

            try
            {
                PublishCommandHandler.PublishContent(
                    source.FullName,
                    destination.FullName,
                    parseResult.GetValue(verboseOption),
                    parseResult.GetValue(noDeleteOption),
                    parseResult.GetValue(showPublishedOption),
                    parseResult.GetValue(previewOption),
                    parseResult.GetValue(excludeDirsOption) ?? []);

                return 0;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or FormatException)
            {
                Console.Error.WriteLine($"Publication failed: {exception.Message}");
                return 1;
            }
        });

        return command;
    }
}
