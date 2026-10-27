using System.CommandLine;
using VaultPublisher.Commands;

namespace VaultPublisher
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Publishes files from a source directory to a target directory");
            
            rootCommand.Subcommands.Add(ConfigCommand.Create());
            rootCommand.Subcommands.Add(PublishCommand.Create());
            
            return rootCommand.Parse(args).Invoke();
        }
    }
}