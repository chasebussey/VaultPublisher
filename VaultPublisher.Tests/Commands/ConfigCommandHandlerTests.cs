using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using VaultPublisher.Commands;
using Xunit;

namespace VaultPublisher.Tests.Commands;

public class ConfigCommandHandlerTests
{
    [Fact]
    public void List_PrintsAllSettings()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }!)
            .Build();
        
        var output = new StringWriter();
        Console.SetOut(output);
        
        ConfigCommandHandler.List(config);
        
        var result = output.ToString();
        
        Assert.Contains("key1: value1", result);
        Assert.Contains("key2: value2", result);
    }
    
    [Fact]
    public void Get_WithKey_PrintsSetting()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }!)
            .Build();
        
        var output = new StringWriter();
        Console.SetOut(output);
        
        ConfigCommandHandler.Get("key1", config);
        
        var result = output.ToString();
        
        Assert.Contains("key1: value1", result);
    }
    
    // TODO: This does save the test configuration to the disk, which is unfortunate. Mock a configuration provider with a dummy Save method.
    [Fact]
    public void Set_SetsSetting()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }!)
            .Build();
        
        ConfigCommandHandler.Set("key1", "new value", config);
        
        Assert.Equal("new value", config["key1"]);
    }
    
    [Fact]
    public void Remove_RemovesSetting()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }!)
            .Build();
        
        ConfigCommandHandler.Remove("key1", config);
        
        Assert.Null(config["key1"]);
    }
}