using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration
    : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ActiveScorerId)
            .IsRequired(false);

        builder.Property(x => x.MaxOvers)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.TossWonTeamId)
            .IsRequired(false);

        builder.Property(x => x.TeamBattingFirstId)
            .IsRequired(false);

        builder.Property(x => x.TeamBattingSecondId)
            .IsRequired(false);

        builder.Property(x => x.CurrentSuperOverNumber)
            .IsRequired();

        builder.Property(x => x.WinnerTeamId)
            .IsRequired(false);

        builder.Property(x => x.Result)
            .IsRequired(false);

        // EF-only FK values used to identify
        // Team 1 and Team 2 PlayingTeam rows.
        builder.Property<int>("Team1TeamId")
            .IsRequired();

        builder.Property<int>("Team2TeamId")
            .IsRequired();

        // Match -> Team 1 PlayingTeam
        builder.HasOne(x => x.Team1PlayingTeam)
            .WithOne()
            .HasForeignKey<Match>(
                "Id",
                "Team1TeamId")
            .HasPrincipalKey<PlayingTeam>(
                "MatchId",
                nameof(PlayingTeam.TeamId))
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> Team 2 PlayingTeam
        builder.HasOne(x => x.Team2PlayingTeam)
            .WithOne()
            .HasForeignKey<Match>(
                "Id",
                "Team2TeamId")
            .HasPrincipalKey<PlayingTeam>(
                "MatchId",
                nameof(PlayingTeam.TeamId))
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> Innings
        builder.HasMany(x => x.Innings)
            .WithOne()
            .HasForeignKey("MatchId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}