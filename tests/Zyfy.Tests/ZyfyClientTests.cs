using System;
using Xunit;

namespace Zyfy.Tests;

public sealed class ZyfyClientTests
{
    [Fact]
    public void Constructor_WithExplicitApiKey_DoesNotThrow()
    {
        using var client = new ZyfyClient("ea_live_test_key");
        Assert.NotNull(client.Vehicle);
        Assert.NotNull(client.Postcode);
    }

    [Fact]
    public void Constructor_WithOptions_UsesApiKey()
    {
        var options = new ZyfyOptions { ApiKey = "ea_live_test_key" };
        using var client = new ZyfyClient(options);
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithEnvVar_UsesApiKey()
    {
        var prev = Environment.GetEnvironmentVariable("ZYFY_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("ZYFY_API_KEY", "ea_live_from_env");
            using var client = new ZyfyClient();
            Assert.NotNull(client);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZYFY_API_KEY", prev);
        }
    }

    [Fact]
    public void Constructor_WithNoKey_ThrowsInvalidOperationException()
    {
        var prev = Environment.GetEnvironmentVariable("ZYFY_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("ZYFY_API_KEY", null);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var client = new ZyfyClient(new ZyfyOptions());
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZYFY_API_KEY", prev);
        }
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            using var client = new ZyfyClient((ZyfyOptions)null!);
        });
    }
}
