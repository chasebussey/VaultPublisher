namespace QuartzPublisher;

public static class Publisher
{
    /// <summary>
    /// Iterates through files in Source directory and copies them to Destination directory
    /// if file contains front matter with publish: true
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="verbose"></param>
    /// <param name="noDelete"></param>
    public static void PublishContent(string source, string destination, bool verbose = false, bool noDelete = false)
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