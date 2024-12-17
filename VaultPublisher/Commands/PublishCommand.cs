using System.CommandLine;

namespace VaultPublisher.Commands;

public static class PublishCommand
{
    public static Command Create()
    {
        var command = new Command("publish", "Publishes the contents of the source directory to the destination directory");
        var config = ConfigurationProvider.GetConfiguration();

        var sourceDirectoryOption = new Option<DirectoryInfo?>(
            name: "--source",
            description: "Source directory for files to publish, e.g. /path/to/vault",
            getDefaultValue: () => ConfigurationProvider.GetDirectory(config["source"])
        );
        sourceDirectoryOption.AddAlias("-s");
        
        var destinationDirectoryOption = new Option<DirectoryInfo?>(
            name: "--destination",
            description: "Destination directory for files to publish, e.g. /path/to/content",
            getDefaultValue: () => ConfigurationProvider.GetDirectory(config["destination"])
        );
        destinationDirectoryOption.AddAlias("-d");
        
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output",
            getDefaultValue: () => false
        );
        verboseOption.AddAlias("-v");
        
        var noDeleteOption = new Option<bool>(
            name: "--no-delete",
            description: "Disable deletion of files not marked for publishing",
            getDefaultValue: () => ConfigurationProvider.GetBool(config["noDelete"], defaultValue: false)
        );
        
        var showPublishedOption = new Option<bool>(
            name: "--show-published",
            description: "Show files that were published",
            getDefaultValue: () => false
        );
        
        var previewOption = new Option<bool>(
            name: "--preview",
            description: "Preview files that would be published and/or deleted without actually publishing them",
            getDefaultValue: () => false
        );

        command.AddOption(sourceDirectoryOption);
        command.AddOption(destinationDirectoryOption);
        command.AddOption(verboseOption);
        command.AddOption(noDeleteOption);
        command.AddOption(showPublishedOption);

        command.SetHandler((source, destination, verbose, noDelete, showPublishedFiles) =>
        {
            if (source is null || destination is null)
            {
                Console.WriteLine("Source and destination directories must be provided.");
                return;
            }
            
            PublishCommandHandler.PublishContent(source?.FullName!, destination?.FullName!, verbose, noDelete, showPublishedFiles);
        }, sourceDirectoryOption, destinationDirectoryOption, verboseOption, noDeleteOption, showPublishedOption);
        
        return command;
    }
}