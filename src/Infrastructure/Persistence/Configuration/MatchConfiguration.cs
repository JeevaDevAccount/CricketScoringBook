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

        // Team 1 PlayingTeam
        builder.OwnsOne(
            x => x.Team1PlayingTeam,
            playingTeam =>
            {
                playingTeam.Property(x => x.TeamId)
                    .HasColumnName("Team1Id")
                    .IsRequired();
            });

        // Team 2 PlayingTeam
        builder.OwnsOne(
            x => x.Team2PlayingTeam,
            playingTeam =>
            {
                playingTeam.Property(x => x.TeamId)
                    .HasColumnName("Team2Id")
                    .IsRequired();
            });
    }
}