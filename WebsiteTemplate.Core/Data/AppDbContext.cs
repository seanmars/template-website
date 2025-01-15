using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Data;

public class AppDbContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
{
    public DbSet<BlockedAccessToken> BlockedAccessTokens { get; set; }
    public DbSet<BlockedRefreshToken> BlockedRefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockedAccessToken>(b =>
        {
            b.HasKey(x => x.Token);

            b.HasIndex(x => x.ExpiryDate);
        });

        modelBuilder.Entity<BlockedRefreshToken>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            b.HasIndex(x => x.Token);
            b.HasIndex(x => x.ExpiryDate);
        });

        base.OnModelCreating(modelBuilder);
    }
}