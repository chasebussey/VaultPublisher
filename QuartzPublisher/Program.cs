﻿using System;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace QuartzPublisher
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (!Path.Exists(options.Source)) throw new ArgumentException("Source directory does not exist");
                    if (!Path.Exists(options.Destination)) throw new ArgumentException("Destination directory does not exist");
                    PublishContent(options.Source, options.Destination, options.Verbose);
                });
        }

        /// <summary>
        /// Iterates through files in Source directory and copies them to Destination directory
        /// if file contains front matter with publish: true
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="verbose"></param>
        private static void PublishContent(string source, string destination, bool verbose = false)
        {
            foreach (var file in Directory.GetFiles(source, "*.md", SearchOption.AllDirectories))
            {
                if (verbose) Console.WriteLine($"Processing {file}");
                if (!ShouldPublish(file, verbose)) continue;
                
                var fileDestination = Path.Join(destination, Path.GetFileName(file));
                
                if (verbose) Console.WriteLine($"Copying {file} to {fileDestination}");
                File.Copy(file, fileDestination, true);
            }
        }
        
        /// <summary>
        /// Determines if a file should be published based on the front matter
        /// Assumes front matter is at the top of the file and is separated by "---"
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if file should be published, else false.</returns>
        private static bool ShouldPublish(string file, bool verbose = false)
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
                    if (verbose) Console.WriteLine($"Publish: {parts[1].Trim('"')}, {shouldPublish}");
                }
            }

            return shouldPublish;
        }
    }

    /// <summary>
    /// Command line options for QuartzPublisher
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class Options
    {
        [Option('s', "source", Required = true, HelpText = "Source directory for files to publish, e.g. /path/to/vault")]
        public required string Source { get; set; }
        
        [Option('d', "destination", Required = true, HelpText = "Destination directory for files to publish, e.g. /path/to/content")]
        public required string Destination { get; set; }
        
        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; set; }
    }
}