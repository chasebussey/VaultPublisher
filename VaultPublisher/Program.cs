using System;
using System.CommandLine;
using Microsoft.Extensions.Configuration;
using VaultPublisher.Commands;

namespace VaultPublisher
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Publishes files from a source directory to a target directory");
            
            rootCommand.AddCommand(ConfigCommand.Create());
            rootCommand.AddCommand(PublishCommand.Create());
            
            return rootCommand.Invoke(args);
        }
    }
}