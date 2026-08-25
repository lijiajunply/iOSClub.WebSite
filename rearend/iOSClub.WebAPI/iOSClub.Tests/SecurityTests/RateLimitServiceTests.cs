using iOSClub.WebAPI.Common.Security;
using Microsoft.Extensions.Logging.Abstractions;

namespace iOSClub.Tests.SecurityTests;

public class RateLimitServiceTests
{
    [Fact]
    public void AuthLoginRoute_UsesLoginPolicy()
    {
        using var service = new RateLimitService(
            new RateLimitConfig(),
            NullLogger<RateLimitService>.Instance);

        Assert.Equal("login", service.GetMatchingPolicy("/Auth/login").Name);
        Assert.Equal("login", service.GetMatchingPolicy("/api/auth/login").Name);
    }

    [Fact]
    public void NonAuthRoute_UsesDefaultPolicy()
    {
        using var service = new RateLimitService(
            new RateLimitConfig(),
            NullLogger<RateLimitService>.Instance);

        Assert.Equal("default", service.GetMatchingPolicy("/Article").Name);
    }
}
