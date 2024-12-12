using VaultPublisher;
using Xunit;

namespace VaultPublisher.Tests;

public class ConfigurationProviderTests
{
    [Fact]
    public void GetConfiguration_ReturnsConfiguration()
    {
        var config = ConfigurationProvider.GetConfiguration();
        
        Assert.NotNull(config);
    }
    
    [Fact]
    public void GetBool_WithNullValue_ReturnsDefaultValue()
    {
        var result = ConfigurationProvider.GetBool(null, true);
        
        Assert.True(result);
    }
    
    [Fact]
    public void GetBool_WithStringValue_ReturnsBool()
    {
        var result = ConfigurationProvider.GetBool("true", false);
        
        Assert.True(result);
    }
    
    [Fact]
    public void GetDirectory_WithNullPath_ReturnsNull()
    {
        var result = ConfigurationProvider.GetDirectory(null);
        
        Assert.Null(result);
    }
    
    [Fact]
    public void GetDirectory_WithStringValue_ReturnsDirectoryInfo()
    {
        var result = ConfigurationProvider.GetDirectory("/path/to/directory");
        
        Assert.NotNull(result);
    }
}