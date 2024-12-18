namespace VaultPublisher.Commands;

public static class PublishCommandHandler
{
    /// <summary>
    /// Iterates through files in Source directory and copies them to Destination directory
    /// if file contains front matter with publish: true
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="verbose"></param>
    /// <param name="noDelete"></param>
    public static void PublishContent(string source, string destination, bool verbose = false, bool noDelete = false, bool showPublishedFiles = false, bool preview = false)
    {
        if (verbose) Console.WriteLine($"Beginning publication of files from {source} to {destination}");
        
        var publishedFiles = new List<string>();
        foreach (var file in Directory.GetFiles(source, "*.md", SearchOption.AllDirectories))
        {
            if (verbose) Console.WriteLine($"Checking {file} for publication frontmatter.");
            if (!ShouldPublish(file)) continue;

            var fileDestination = Path.Join(destination, Path.GetFileName(file));

            if (verbose) Console.WriteLine($"Copying {file} to {fileDestination}");
            if (!preview) File.Copy(file, fileDestination, true);
            publishedFiles.Add(fileDestination);
        }

        if (verbose && !noDelete)
        {
            Console.WriteLine("Publication complete.");
            WriteSeparator();
        }

        if (noDelete)
        {
            if (verbose)
            {
                Console.WriteLine("Skipping deletion of files not marked for publishing.");
                WriteSeparator();
    
                Console.WriteLine("Published files: ");
                foreach (var file in publishedFiles)
                {
                    Console.WriteLine(file);
                }
                WriteSeparator();
            }
            
            Console.WriteLine($"Published {publishedFiles.Count} files to {destination}");
            
            return;
        }

        if (verbose) Console.WriteLine($"Checking for files to delete.");
        
        var deletedFiles = new List<string>();
        foreach (var file in Directory.GetFiles(destination, "*.md", SearchOption.AllDirectories))
        {
            var sourceFile = Path.Join(source, Path.GetFileName(file));
            if (ShouldPublish(sourceFile)) continue;

            if (verbose) Console.WriteLine($"Deleting {file}");
            if (!preview) File.Delete(file);
            deletedFiles.Add(file);
        }

        if (verbose)
        {
            Console.WriteLine("Deletion complete.");
            WriteSeparator();
            
            Console.WriteLine("Published files: ");
            foreach (var file in publishedFiles)
            {
                Console.WriteLine(file);
            }
            WriteSeparator();

            Console.WriteLine("Deleted files: ");
            foreach (var file in deletedFiles)
            {
                Console.WriteLine(file);
            }
            WriteSeparator();
        }
        
        if (!preview) Console.WriteLine($"Published {publishedFiles.Count} files and deleted {deletedFiles.Count} files from {destination}");
        if (preview) Console.WriteLine($"Would publish {publishedFiles.Count} files and delete {deletedFiles.Count} files from {destination}");
        
        if (showPublishedFiles)
        {
            WriteSeparator();
            Console.WriteLine("Published files: ");
            foreach (var file in publishedFiles)
            {
                Console.WriteLine(file);
            }
        }
    }

    /// <summary>
    /// Determines if a file should be published based on the front matter
    /// Assumes front matter is at the top of the file and is separated by "---"
    /// </summary>
    /// <param name="file"></param>
    /// <returns>true if file should be published, else false.</returns>
    private static bool ShouldPublish(string file)
    {
        if (!File.Exists(file)) return false;
        
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
    
    private static void WriteSeparator() => Console.WriteLine(new string('-', 80));
}