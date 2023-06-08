namespace BuildContainersFeature.Tests;

using Microsoft.AspNetCore.Mvc.Testing;

public class UnitTest
{
    [Fact]
    public async Task HelloWorldTests()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var response = await client.GetStringAsync("/");
    
        Assert.Equal("Hello World!", response);
    }
}