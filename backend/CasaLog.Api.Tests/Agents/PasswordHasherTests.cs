using CasaLog.Api.Infrastructure.Security;

namespace CasaLog.Api.Tests.Agents;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsNonPlaintextString()
    {
        var hash = PasswordHasher.Hash("mypassword");
        Assert.NotEqual("mypassword", hash);
        Assert.Contains(".", hash);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("correct");
        Assert.True(PasswordHasher.Verify("correct", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("correct");
        Assert.False(PasswordHasher.Verify("wrong", hash));
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        var h1 = PasswordHasher.Hash("password");
        var h2 = PasswordHasher.Hash("password");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void Verify_InvalidFormat_ReturnsFalse()
    {
        Assert.False(PasswordHasher.Verify("password", "notavalidhash"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("a very long password with special chars !@#$%")]
    [InlineData("12345678")]
    public void Hash_AndVerify_WorksForVariousPasswords(string password)
    {
        var hash = PasswordHasher.Hash(password);
        Assert.True(PasswordHasher.Verify(password, hash));
    }

    [Fact]
    public void CreateHomeHandler_MapToTasks_InvalidDate_Skipped()
    {
        var items = new[]
        {
            new CasaLog.Api.Agents.AgentTaskItem("ac_cleaning", "not-a-date", "high", "test", null),
            new CasaLog.Api.Agents.AgentTaskItem("drain_cleaning", "2026-08-01", "low", "test", null)
        };
        var tasks = CasaLog.Api.Features.Homes.CreateHomeHandler.MapToTasks(items, Guid.NewGuid(), []);
        Assert.Single(tasks);
        Assert.Equal("drain_cleaning", tasks[0].Type);
    }
}
