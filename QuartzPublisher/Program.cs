using System;
using System.CommandLine;

namespace QuartzPublisher
{
    public static class Program
    {
        private static int Main(string[] args)
        {
            var sourceDirectoryOption = new Option<DirectoryInfo?>(
                name: "--source",
                description: "Source directory for files to publish, e.g. /path/to/vault"
            );
            sourceDirectoryOption.AddAlias("-s");
            
            var destinationDirectoryOption = new Option<DirectoryInfo?>(
                name: "--destination",
                description: "Destination directory for files to publish, e.g. /path/to/content"
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
                getDefaultValue: () => false
            );
            
            
            var rootCommand = new RootCommand("Publishes files from a source directory to a target directory")
            {
                sourceDirectoryOption,
                destinationDirectoryOption,
                verboseOption,
                noDeleteOption
            };
            
            rootCommand.SetHandler((DirectoryInfo? source, DirectoryInfo? destination, bool verbose, bool noDelete) =>
            {
                if (source is null || destination is null)
                {
                    Console.WriteLine("Source and destination directories must be provided.");
                    return;
                }
                
                PublishContent(source?.FullName, destination?.FullName, verbose, noDelete);
            }, sourceDirectoryOption, destinationDirectoryOption, verboseOption, noDeleteOption);

            return rootCommand.Invoke(args);
        }

        /// <summary>
        /// Iterates through files in Source directory and copies them to Destination directory
        /// if file contains front matter with publish: true
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="verbose"></param>
        private static void PublishContent(string source, string destination, bool verbose = false, bool noDelete = false)
        {
            foreach (var file in Directory.GetFiles(source, "*.md", SearchOption.AllDirectories))
            {
                if (verbose) Console.WriteLine($"Processing {file}");
                if (!ShouldPublish(file)) continue;
                
                var fileDestination = Path.Join(destination, Path.GetFileName(file));
                
                if (verbose) Console.WriteLine($"Copying {file} to {fileDestination}");
                File.Copy(file, fileDestination, true);
            }

            if (noDelete)
            {
                if (!verbose) return;
                
                Console.WriteLine("Publishing complete.");
                
                return;
            }
            
            if (verbose) Console.WriteLine($"Copying files to {destination} complete. Checking for files to delete.");
            
            foreach (var file in Directory.GetFiles(destination, "*.md", SearchOption.AllDirectories))
            {
                var sourceFile = Path.Join(source, Path.GetFileName(file));
                if (ShouldPublish(sourceFile)) continue;
                
                if (verbose) Console.WriteLine($"Deleting {file}");
                File.Delete(file);
            }

            if (!verbose) return;
            
            Console.WriteLine("Deletion complete.");
            Console.WriteLine("Publishing complete.");
        }
        
        /// <summary>
        /// Determines if a file should be published based on the front matter
        /// Assumes front matter is at the top of the file and is separated by "---"
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if file should be published, else false.</returns>
        private static bool ShouldPublish(string file)
        {
            var shouldPublish = false;
            using var reader = new StreamReader(file);
            var inFrontmatter = false;
            while (reader.ReadLine() is { } line)
            {
                if (line == "---")
                {
                    inFrontmatter = !inFrontmatter;
                }

                if (!inFrontmatter) break;
                var parts = line.Split(": ");
                if (string.Equals(parts[0], "publish", StringComparison.InvariantCultureIgnoreCase))
                {
                    shouldPublish = bool.Parse(parts[1].Trim('"'));
                }
            }

            return shouldPublish;
        }
    }
}