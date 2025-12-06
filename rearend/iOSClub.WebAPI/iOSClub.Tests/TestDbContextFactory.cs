using iOSClub.Data;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class TestDbContextFactory(DbContextOptions<ClubContext> options) : IDbContextFactory<ClubContext>
{
    public ClubContext CreateDbContext()
    {
        return new ClubContext(options);
    }

    public async ValueTask<ClubContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(CreateDbContext());
    }
}