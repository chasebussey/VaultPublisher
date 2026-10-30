namespace VaultPublisher.Commands;

public static class PublishCommandHandler
{
    // Be conservative on platforms that commonly use case-insensitive filesystems.
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    private static readonly StringComparer PathComparer = PathComparison == StringComparison.OrdinalIgnoreCase
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    /// <summary>
    /// Synchronizes publishable Markdown by relative path. Validation and planning finish
    /// before any writes; preview reports the same plan without changing the filesystem.
    /// </summary>
    public static void PublishContent(string source, string destination, bool verbose = false, bool noDelete = false,
        bool showPublishedFiles = false, bool preview = false, string[]? excludeDirs = null)
    {
        var sourceRoot = NormalizeRoot(source);
        var destinationRoot = NormalizeRoot(destination);
        ValidateRoots(sourceRoot, destinationRoot);

        var exclusions = new HashSet<string>(excludeDirs ?? [], PathComparer);
        var plan = BuildPlan(sourceRoot, destinationRoot, exclusions, noDelete, verbose);

        foreach (var copy in plan.Copies)
        {
            if (verbose) Console.WriteLine($"{(preview ? "Would copy" : "Copying")} {copy.Source} to {copy.Destination}");
            if (preview) continue;

            Directory.CreateDirectory(Path.GetDirectoryName(copy.Destination)!);
            File.Copy(copy.Source, copy.Destination, overwrite: true);
        }

        foreach (var file in plan.Deletions)
        {
            if (verbose) Console.WriteLine($"{(preview ? "Would delete" : "Deleting")} {file}");
            if (!preview) File.Delete(file);
        }

        var summary = preview ? "Would publish" : "Published";
        var deletionSummary = preview ? "delete" : "deleted";
        Console.WriteLine($"{summary} {plan.Copies.Count} files and {deletionSummary} {plan.Deletions.Count} files from {destinationRoot}");

        if (showPublishedFiles || preview)
        {
            WriteFiles(preview ? "Files to publish:" : "Published files:", plan.Copies.Select(copy => copy.Destination));
        }

        if (verbose || preview)
        {
            WriteFiles(preview ? "Files to delete:" : "Deleted files:", plan.Deletions);
        }
    }

    private static void ValidateRoots(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {source}");
        }

        if (File.Exists(destination))
        {
            throw new IOException($"Destination must be a directory: {destination}");
        }

        if (IsWithin(source, destination) || IsWithin(destination, source))
        {
            throw new ArgumentException("Source and destination directories must not be identical or nested within one another.");
        }
    }

    private static bool IsWithin(string path, string root)
    {
        var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        return PathComparer.Equals(path, root) || path.StartsWith(prefix, PathComparison);
    }

    private static string NormalizeRoot(string path)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var current = Path.GetPathRoot(fullPath)!;
        var segments = fullPath[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        for (var index = 0; index < segments.Length; index++)
        {
            current = Path.Combine(current, segments[index]);
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(current);
            }
            catch (FileNotFoundException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            if (index == segments.Length - 1)
            {
                RejectLink(current, attributes);
            }
            else if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                // Resolve parent aliases (e.g. macOS /tmp) before checking root overlap.
                var target = new DirectoryInfo(current).ResolveLinkTarget(returnFinalTarget: true)
                    ?? throw new IOException($"Cannot resolve directory alias: {current}");
                current = NormalizeRoot(target.FullName);
            }
            else if ((attributes & FileAttributes.Directory) == 0)
            {
                throw new IOException($"A file blocks the directory path: {current}");
            }
        }

        return current;
    }

    private static void RejectLink(string path, FileAttributes attributes)
    {
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException($"Symbolic links and directory junctions are not supported: {path}");
        }
    }

    private static PublishPlan BuildPlan(string source, string destination, HashSet<string> exclusions, bool noDelete, bool verbose)
    {
        // Inspect the entire existing destination, even with --no-delete, before reading or overwriting files.
        var destinationFiles = Directory.Exists(destination)
            ? EnumerateMarkdown(destination, []).ToArray()
            : [];
        var publishedPaths = new HashSet<string>(PathComparer);
        var copies = new List<CopyOperation>();

        foreach (var file in EnumerateMarkdown(source, exclusions))
        {
            if (verbose) Console.WriteLine($"Checking {file} for publication frontmatter.");
            if (!ShouldPublish(file)) continue;

            var relativePath = Path.GetRelativePath(source, file);
            if (!publishedPaths.Add(relativePath))
            {
                throw new IOException($"Publishable paths differ only by case: {relativePath}");
            }

            var target = Path.Combine(destination, relativePath);
            ValidateCopyTarget(target, destination);
            if (File.Exists(target) && File.ReadAllText(file) == File.ReadAllText(target))
            {
                if (verbose) Console.WriteLine($"Skipping {file} which is unchanged.");
                continue;
            }

            copies.Add(new CopyOperation(file, target));
        }

        var deletions = noDelete
            ? []
            : destinationFiles.Where(file => !publishedPaths.Contains(Path.GetRelativePath(destination, file))).ToList();

        return new PublishPlan(copies, deletions);
    }

    private static IEnumerable<string> EnumerateMarkdown(string root, HashSet<string> exclusions)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.TryPop(out var directory))
        {
            foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
            {
                var attributes = entry.Attributes;
                RejectLink(entry.FullName, attributes);

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if (!exclusions.Contains(entry.Name)) pending.Push(entry.FullName);
                }
                else if (string.Equals(entry.Extension, ".md", StringComparison.OrdinalIgnoreCase))
                {
                    yield return entry.FullName;
                }
            }
        }
    }

    private static void ValidateCopyTarget(string target, string destination)
    {
        if (Directory.Exists(target))
        {
            throw new IOException($"Cannot publish a file over a directory: {target}");
        }

        for (var parent = Path.GetDirectoryName(target); parent is not null && !PathComparer.Equals(parent, destination);
             parent = Path.GetDirectoryName(parent))
        {
            if (File.Exists(parent))
            {
                throw new IOException($"A file blocks the destination directory: {parent}");
            }
        }
    }

    private static bool ShouldPublish(string file)
    {
        using var reader = new StreamReader(file);
        if (reader.ReadLine() != "---") return false;

        var shouldPublish = false;
        while (reader.ReadLine() is { } line && line != "---")
        {
            var separator = line.IndexOf(':');
            if (separator < 0 || !line[..separator].Trim().Equals("publish", StringComparison.OrdinalIgnoreCase)) continue;

            var value = line[(separator + 1)..].Trim().Trim('"');
            if (!bool.TryParse(value, out shouldPublish))
            {
                throw new FormatException($"Invalid publish value in {file}: {value}");
            }
        }

        return shouldPublish;
    }

    private static void WriteFiles(string heading, IEnumerable<string> files)
    {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine(heading);
        foreach (var file in files) Console.WriteLine(file);
    }

    private sealed record CopyOperation(string Source, string Destination);
    private sealed record PublishPlan(List<CopyOperation> Copies, List<string> Deletions);
}
