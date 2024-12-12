using System;
using System.CommandLine;
using System.IO;
using VaultPublisher;
using VaultPublisher.Commands;
using Xunit;

namespace VaultPublisher.Tests
{
    public class PublishCommandHandlerTests : PublishCommandHandlerTestsBase
    {
        [Fact]
        public void PublishContent_WithSourceAndDestinationDirectories_CopiesFiles()
        {
            // Arrange
            var source = SourceDirectory.FullName;
            var destination = DestinationDirectory.FullName;
            
            // Act
            PublishCommandHandler.PublishContent(source, destination);
            
            // Assert
            Assert.True(File.Exists(Path.Combine(destination, ShouldPublishPath)));
            Assert.False(File.Exists(Path.Combine(destination, NoPublishPath)));
        }
    }

    public abstract class PublishCommandHandlerTestsBase : IDisposable
    {
        protected DirectoryInfo SourceDirectory { get; }
        protected DirectoryInfo DestinationDirectory { get; }

        private const string ShouldPublishText = """
                                                 ---
                                                 publish: true
                                                 ---
                                                 This file should be published.
                                                 """;

        private const string NoPublishText = """
                                             ---
                                             publish: false
                                             ---
                                             This file should not be published.
                                             """;
        
        protected const string ShouldPublishPath = "testPublish.md";
        protected const string NoPublishPath = "testNoPublish.md";
        
        protected PublishCommandHandlerTestsBase()
        {
            // Set up test directories
            SourceDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "source"));
            DestinationDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "destination"));
            
            SourceDirectory.Create();
            DestinationDirectory.Create();
            
            // Create test files
            File.WriteAllText(Path.Combine(SourceDirectory.FullName, ShouldPublishPath), ShouldPublishText);
            File.WriteAllText(Path.Combine(SourceDirectory.FullName, NoPublishPath), NoPublishText);
        }
        
        public void Dispose()
        {
            // Clean up test directories
            SourceDirectory.Delete(true);
            DestinationDirectory.Delete(true);
        }
    }
}