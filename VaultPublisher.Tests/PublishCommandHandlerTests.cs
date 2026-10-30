using VaultPublisher.Commands;

namespace VaultPublisher.Tests;

public sealed class PublishCommandHandlerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"vaultpublisher-{Guid.NewGuid():N}");
    private string Source => Path.Combine(root, "source");
    private string Destination => Path.Combine(root, "destination");

    public PublishCommandHandlerTests()
    {
        Directory.CreateDirectory(Source);
    }

    [Fact]
    public void Publish_PreservesRelativePathsAndSkipsUnchangedFiles()
    {
        Write(Source, "note.md", Note("root"));
        Write(Source, "projects/note.md", Note("project"));
        Write(Source, "projects/research/note.md", Note("research"));
        Write(Source, "unchanged.md", Note("unchanged"));
        Write(Source, "private.md", "---\npublish: false\n---");
        Write(Source, "image.png", "source image");
        Directory.CreateDirectory(Path.Combine(Source, "empty"));

        Write(Destination, "projects/note.md", "Old content");
        Write(Destination, "unchanged.md", Note("unchanged"));
        var unchanged = Path.Combine(Destination, "unchanged.md");
        File.SetLastWriteTimeUtc(unchanged, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var originalTimestamp = File.GetLastWriteTimeUtc(unchanged);

        PublishCommandHandler.PublishContent(Source, Destination);

        Assert.Equal(new[] { "note.md", "projects/note.md", "projects/research/note.md", "unchanged.md" }, RelativeFiles(Destination));
        Assert.Equal(Note("root"), File.ReadAllText(Path.Combine(Destination, "note.md")));
        Assert.Equal(Note("project"), File.ReadAllText(Path.Combine(Destination, "projects/note.md")));
        Assert.Equal(Note("research"), File.ReadAllText(Path.Combine(Destination, "projects/research/note.md")));
        Assert.Equal(originalTimestamp, File.GetLastWriteTimeUtc(unchanged));
        Assert.False(Directory.Exists(Path.Combine(Destination, "empty")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Publish_DeletionUsesExactPathsAndHonorsExclusionsAndNoDelete(bool noDelete)
    {
        Write(Source, "note.md", Note("root"));
        Write(Source, "nested/keep.md", Note("keep"));
        Write(Source, "unpublished.md", "---\npublish: false\n---");
        Write(Source, "private/secret.md", Note("excluded"));
        Write(Source, "nested/private/deep/secret.md", Note("also excluded"));

        string[] stale = ["nested/note.md", "removed.md", "unpublished.md", "private/secret.md", "nested/private/deep/secret.md"];
        foreach (var path in stale) Write(Destination, path, "Previously published");
        Write(Destination, "asset.png", "Site asset");
        Directory.CreateDirectory(Path.Combine(Destination, "empty"));

        PublishCommandHandler.PublishContent(Source, Destination, noDelete: noDelete, excludeDirs: ["private"]);

        Assert.Equal(Note("keep"), File.ReadAllText(Path.Combine(Destination, "nested/keep.md")));
        Assert.True(File.Exists(Path.Combine(Destination, "note.md")));
        foreach (var path in stale) Assert.Equal(noDelete, File.Exists(Path.Combine(Destination, path)));
        Assert.Equal("Site asset", File.ReadAllText(Path.Combine(Destination, "asset.png")));
        Assert.True(Directory.Exists(Path.Combine(Destination, "empty")));
        Assert.DoesNotContain("secret.md", RelativeFiles(Destination));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preview_DoesNotChangeEitherTreeIncludingMissingDestination(bool destinationExists)
    {
        Write(Source, "deep/note.md", Note("new"));
        if (destinationExists) Write(Destination, "stale.md", "Existing content");
        var before = Snapshot();

        PublishCommandHandler.PublishContent(Source, Destination, preview: true);

        Assert.Equal(before, Snapshot());
        Assert.Equal(destinationExists, Directory.Exists(Destination));
    }

    [Theory]
    [InlineData("same")]
    [InlineData("destination-inside-source")]
    [InlineData("source-inside-destination")]
    [InlineData("missing-source")]
    public void Publish_RejectsInvalidRootsBeforeChanges(string scenario)
    {
        Write(Destination, "retained.md", "Existing content");
        var source = scenario == "missing-source" ? Path.Combine(root, "missing") : Source;
        var destination = Destination;
        if (scenario == "same") destination = Source + Path.DirectorySeparatorChar;
        if (scenario == "destination-inside-source") destination = Path.Combine(Source, "new-output");
        if (scenario == "source-inside-destination") destination = root;
        var before = Snapshot();

        if (scenario == "missing-source")
        {
            Assert.Throws<DirectoryNotFoundException>(() => PublishCommandHandler.PublishContent(source, destination));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => PublishCommandHandler.PublishContent(source, destination));
        }

        Assert.Equal(before, Snapshot());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Publish_PlanningFailureDoesNotPartiallyPublishOrDelete(bool blockedTarget)
    {
        Write(Source, "valid.md", Note("valid"));
        Write(Destination, "retained.md", "Existing content");
        if (blockedTarget)
        {
            Write(Source, "blocked/note.md", Note("blocked"));
            Write(Destination, "blocked", "A file, not a directory");
        }
        else
        {
            Write(Source, "invalid.md", "---\npublish: not-a-boolean\n---");
        }
        var before = Snapshot();

        if (blockedTarget)
        {
            Assert.Throws<IOException>(() => PublishCommandHandler.PublishContent(Source, Destination));
        }
        else
        {
            Assert.Throws<FormatException>(() => PublishCommandHandler.PublishContent(Source, Destination));
        }

        Assert.Equal(before, Snapshot());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Publish_RejectsLinkedFilesAndDirectoriesInEitherTree(bool inDestination, bool directoryLink)
    {
        var outside = Path.Combine(root, "outside");
        Write(outside, "note.md", Note("Must not change"));
        Write(Source, "valid.md", Note("valid"));
        Write(Destination, "retained.md", "Existing content");
        var link = Path.Combine(inDestination ? Destination : Source, directoryLink ? "linked" : "linked.md");
        if (directoryLink) Directory.CreateSymbolicLink(link, outside);
        else File.CreateSymbolicLink(link, Path.Combine(outside, "note.md"));

        try
        {
            Assert.Throws<IOException>(() => PublishCommandHandler.PublishContent(Source, Destination, noDelete: true));
            Assert.False(File.Exists(Path.Combine(Destination, "valid.md")));
            Assert.Equal("Existing content", File.ReadAllText(Path.Combine(Destination, "retained.md")));
            Assert.Equal(Note("Must not change"), File.ReadAllText(Path.Combine(outside, "note.md")));
        }
        finally
        {
            if (directoryLink) Directory.Delete(link);
            else File.Delete(link);
        }
    }

    [Fact]
    public void Publish_RejectsLinkedRootsAndDetectsOverlapThroughParentAliases()
    {
        var alias = Path.Combine(root, "alias");
        Directory.CreateSymbolicLink(alias, Source);

        try
        {
            Assert.Throws<ArgumentException>(() => PublishCommandHandler.PublishContent(Source, Path.Combine(alias, "new-output")));
            Assert.Throws<IOException>(() => PublishCommandHandler.PublishContent(alias, Destination));
            Assert.False(Directory.Exists(Path.Combine(Source, "new-output")));
            Assert.False(Directory.Exists(Destination));
        }
        finally
        {
            Directory.Delete(alias);
        }
    }

    private static string Note(string content) => $"---\npublish: true\n---\n{content}";

    private static void Write(string directory, string relativePath, string content)
    {
        var path = Path.Combine(directory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string[] RelativeFiles(string directory) => Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(directory, path).Replace(Path.DirectorySeparatorChar, '/'))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

    private string[] Snapshot() => Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => Directory.Exists(path) ? $"directory:{path}" : $"file:{path}:{File.ReadAllText(path)}")
        .ToArray();

    public void Dispose() => Directory.Delete(root, recursive: true);
}
