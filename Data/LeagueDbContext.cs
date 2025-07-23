using LeagueManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Data;

public class LeagueDbContext : DbContext
{
    public LeagueDbContext(DbContextOptions<LeagueDbContext> options) : base(options) { }

    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Fixture> Fixtures { get; set; }
    public DbSet<Location> Locations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Fixture>()
            .HasOne(f => f.HomeTeam)
            .WithMany()
            .HasForeignKey(f => f.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        
        modelBuilder.Entity<Fixture>()
            .HasOne(f => f.AwayTeam)
            .WithMany()
            .HasForeignKey(f => f.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}