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
    /// <param name="showPublishedFiles"></param>
    /// <param name="preview"></param>
    /// <param name="excludeDirs"></param>
    public static void PublishContent(string source, string destination, bool verbose = false, bool noDelete = false,
        bool showPublishedFiles = false, bool preview = false, string[]? excludeDirs = null)
    {
        if (verbose) Console.WriteLine($"Beginning publication of files from {source} to {destination}");
        
        var publishedFiles = new List<string>();
        var deletedFiles = new List<string>();
        var directories = Directory.GetDirectories(source)
            .Where(dir => excludeDirs is null || !excludeDirs.Contains(Path.GetFileName(dir)));
        directories = directories.Append(source).ToArray();
        
        foreach (var child in directories)
        {
            var dirName = child == source ? string.Empty : Path.GetFileName(child);
            var childDestination = Path.Join(destination, dirName);
            if (verbose) Console.WriteLine($"Publishing files from {child} to {childDestination}");
            
            var (published, deleted) = PublishDirectory(child, childDestination, verbose, noDelete, preview);
            
            publishedFiles.AddRange(published);
            deletedFiles.AddRange(deleted);
        }
        
        if (!preview) Console.WriteLine($"Published {publishedFiles.Count} files and deleted {deletedFiles.Count} files from {destination}");
        if (preview) Console.WriteLine($"Would publish {publishedFiles.Count} files and delete {deletedFiles.Count} files from {destination}");
        
        if (showPublishedFiles || preview)
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
    /// Takes a source directory and copies files therein to a matching child directory of the given destination. If no matching child directory exists, it is created.
    /// Returns a list of files that were published and a list of files that were deleted.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="verbose"></param>
    /// <param name="noDelete"></param>
    /// <param name="preview"></param>
    /// <returns>Tuple containing the list of files published and the list of files deleted.</returns>
    private static (List<string>, List<string>) PublishDirectory(string source, string destination, bool verbose = false, bool noDelete = false, bool preview = false)
    {
        var publishedFiles = new List<string>();
        
        if (!Directory.Exists(destination))
        {
            if (verbose) Console.WriteLine($"Creating directory {destination}");
            if (!preview) Directory.CreateDirectory(destination);
        }
        
        foreach (var file in Directory.GetFiles(source, "*.md", SearchOption.AllDirectories))
        {
            if (verbose) Console.WriteLine($"Checking {file} for publication frontmatter.");
            if (!ShouldPublish(file)) continue;

            var fileDestination = Path.Join(destination, Path.GetFileName(file));
            
            if (File.Exists(fileDestination))
            {
                var sourceContent = File.ReadAllText(file);
                var destContent = File.ReadAllText(fileDestination);

                if (sourceContent == destContent)
                {
                    if (verbose) Console.WriteLine($"Skipping {file} which is unchanged.");
                    
                    continue;
                }
            }

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
            
            return (publishedFiles, []);
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

        return (publishedFiles, deletedFiles);
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