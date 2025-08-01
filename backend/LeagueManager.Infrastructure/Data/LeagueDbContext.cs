using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Data;

public class LeagueDbContext : IdentityDbContext<User>
{
    public LeagueDbContext(DbContextOptions<LeagueDbContext> options) : base(options) { }

    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Fixture> Fixtures { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<TeamMembership> TeamMemberships { get; set; }
    public DbSet<MOMVote> MOMVotes { get; set; }

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

        modelBuilder.Entity<MOMVote>()
            .HasOne(v => v.VotingTeam)
            .WithMany()
            .HasForeignKey(v => v.VotingTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MOMVote>()
            .HasOne(v => v.VotedForOwnPlayer)
            .WithMany()
            .HasForeignKey(v => v.VotedForOwnPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MOMVote>()
            .HasOne(v => v.VotedForOpponentPlayer)
            .WithMany()
            .HasForeignKey(v => v.VotedForOpponentPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}