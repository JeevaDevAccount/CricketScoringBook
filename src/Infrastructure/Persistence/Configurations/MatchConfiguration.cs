using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ActiveScorerId).IsRequired(false);
        builder.Property(x => x.ConcurrencyVersion).IsRequired().IsConcurrencyToken();
        builder.Property(x => x.MaxOvers).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.TossWonTeamId).IsRequired(false);
        builder.Property(x => x.TeamBattingFirstId).IsRequired(false);
        builder.Property(x => x.TeamBattingSecondId).IsRequired(false);
        builder.Property(x => x.CurrentSuperOverNumber).IsRequired();
        builder.Property(x => x.WinnerTeamId).IsRequired(false);
        builder.Property(x => x.Result).IsRequired(false);

        // 🌟 Team 1 Mapping: Clean, isolated table mapping
        builder.OwnsOne(x => x.Team1PlayingTeam, team1 =>
        {
            team1.ToTable("MatchTeam1Details"); // 👈 Distinct table name
            team1.WithOwner().HasForeignKey("MatchId");
            team1.HasKey("MatchId"); 

            team1.Property(x => x.TeamId).IsRequired();

            team1.OwnsMany(x => x.Players, player =>
            {
                player.ToTable("MatchTeam1Players"); // 👈 Distinct table name
                player.WithOwner().HasForeignKey("MatchId");
                player.HasKey("MatchId", nameof(PlayingTeamPlayer.PlayerId));
                player.Property(x => x.PlayerId).IsRequired();
            });

            team1.Navigation(x => x.Players)
                .HasField("_players")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(x => x.Team1PlayingTeam)
            .HasField("_team1PlayingTeam")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // 🌟 Team 2 Mapping: Clean, isolated table mapping
        builder.OwnsOne(x => x.Team2PlayingTeam, team2 =>
        {
            team2.ToTable("MatchTeam2Details"); // 👈 Distinct table name
            team2.WithOwner().HasForeignKey("MatchId");
            team2.HasKey("MatchId"); 

            team2.Property(x => x.TeamId).IsRequired();

            team2.OwnsMany(x => x.Players, player =>
            {
                player.ToTable("MatchTeam2Players"); // 👈 Distinct table name
                player.WithOwner().HasForeignKey("MatchId");
                player.HasKey("MatchId", nameof(PlayingTeamPlayer.PlayerId));
                player.Property(x => x.PlayerId).IsRequired();
            });

            team2.Navigation(x => x.Players)
                .HasField("_players")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(x => x.Team2PlayingTeam)
            .HasField("_team2PlayingTeam")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Innings)
            .WithOne()
            .HasForeignKey("MatchId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}