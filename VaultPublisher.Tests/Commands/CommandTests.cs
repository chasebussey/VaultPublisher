using System.CommandLine;
using Microsoft.Extensions.Configuration;
using VaultPublisher.Commands;

namespace VaultPublisher.Tests.Commands;

public class CommandTests
{
    [Fact]
    public void Publish_UsesSavedDefaults()
    {
        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["source"] = Path.GetTempPath(),
            ["destination"] = Path.GetTempPath(),
            ["noDelete"] = "true",
            ["excludeDirs"] = "private,drafts"
        });
        var command = PublishCommand.Create(config);
        var result = command.Parse([]);

        Assert.Empty(result.Errors);
        Assert.Equal(Path.GetFullPath(Path.GetTempPath()), result.GetValue(GetOption<DirectoryInfo?>(command, "--source"))!.FullName);
        Assert.True(result.GetValue(GetOption<bool>(command, "--no-delete")));
        Assert.Equal(new[] { "private", "drafts" }, result.GetValue(GetOption<string[]>(command, "--exclude-dirs")));
        Assert.False(result.GetValue(GetOption<bool>(command, "--preview")));
        Assert.False(result.GetValue(GetOption<bool>(command, "--verbose")));
        Assert.False(result.GetValue(GetOption<bool>(command, "--show-published")));
    }

    [Fact]
    public void Publish_CommandLineOverridesSavedDefaults()
    {
        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["source"] = Path.Combine(Path.GetTempPath(), "saved-source"),
            ["destination"] = Path.Combine(Path.GetTempPath(), "saved-destination"),
            ["noDelete"] = "true",
            ["excludeDirs"] = "private,drafts"
        });
        var command = PublishCommand.Create(config);
        var result = command.Parse([
            "-s", Path.GetTempPath(), "-d", Path.GetTempPath(), "-v",
            "--no-delete", "false", "--preview", "--show-published", "--exclude-dirs", "archive"
        ]);

        Assert.Empty(result.Errors);
        Assert.Equal(Path.GetFullPath(Path.GetTempPath()), result.GetValue(GetOption<DirectoryInfo?>(command, "--source"))!.FullName);
        Assert.Equal(Path.GetFullPath(Path.GetTempPath()), result.GetValue(GetOption<DirectoryInfo?>(command, "--destination"))!.FullName);
        Assert.False(result.GetValue(GetOption<bool>(command, "--no-delete")));
        Assert.True(result.GetValue(GetOption<bool>(command, "--preview")));
        Assert.True(result.GetValue(GetOption<bool>(command, "--verbose")));
        Assert.True(result.GetValue(GetOption<bool>(command, "--show-published")));
        Assert.Equal(new[] { "archive" }, result.GetValue(GetOption<string[]>(command, "--exclude-dirs")));
    }

    [Theory]
    [InlineData("--no-delete", "invalid")]
    [InlineData("--unknown", "value")]
    [InlineData("--source", null)]
    public void Publish_InvalidArgumentsAreRejected(string option, string? value)
    {
        var command = PublishCommand.Create(CreateConfiguration());
        var args = value is null ? new[] { option } : [option, value];

        Assert.NotEmpty(command.Parse(args).Errors);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("set")]
    [InlineData("set source")]
    [InlineData("remove")]
    public void Config_RequiresArguments(string arguments)
    {
        var command = ConfigCommand.Create(() => CreateConfiguration());

        Assert.NotEmpty(command.Parse(arguments).Errors);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("get source")]
    public void Config_ReadActionsLoadConfigurationOnInvocation(string arguments)
    {
        var loadCount = 0;
        var command = ConfigCommand.Create(() =>
        {
            loadCount++;
            return CreateConfiguration();
        });
        var result = command.Parse(arguments);

        Assert.Empty(result.Errors);
        Assert.Equal(0, loadCount);
        Assert.Equal(0, result.Invoke());
        Assert.Equal(1, loadCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Publish_ActionPreservesPreviewAndNoDeleteSafety(bool preview)
    {
        var root = Path.Combine(Path.GetTempPath(), $"vaultpublisher-cli-{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var destination = Path.Combine(root, "destination");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(destination);

        try
        {
            File.WriteAllText(Path.Combine(source, "note.md"), "---\npublish: true\n---\nPublished note");
            var retainedFile = Path.Combine(destination, "retained.md");
            File.WriteAllText(retainedFile, "Existing content");

            var command = PublishCommand.Create(CreateConfiguration());
            var safetyOption = preview ? "--preview" : "--no-delete";
            var result = command.Parse(["-s", source, "-d", destination, safetyOption]);

            Assert.Empty(result.Errors);
            Assert.Equal(0, result.Invoke());
            Assert.Equal("Existing content", File.ReadAllText(retainedFile));
            Assert.Equal(!preview, File.Exists(Path.Combine(destination, "note.md")));
            Assert.Single(Directory.GetFiles(source));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?>? values = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static Option<T> GetOption<T>(Command command, string name) =>
        command.Options.OfType<Option<T>>().Single(option => option.Name == name);
}
