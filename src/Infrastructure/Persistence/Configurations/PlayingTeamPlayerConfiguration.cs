using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PlayingTeamPlayerConfiguration
    : IEntityTypeConfiguration<PlayingTeamPlayer>
{
    public void Configure(
        EntityTypeBuilder<PlayingTeamPlayer> builder)
    {
        builder.ToTable("PlayingTeamPlayers");

        builder.Property<Guid>("MatchId")
            .IsRequired();

        builder.Property<int>("TeamId")
            .IsRequired();

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.HasKey(
            "MatchId",
            "TeamId",
            nameof(PlayingTeamPlayer.PlayerId));

        builder.HasOne<PlayingTeam>()
            .WithMany(x => x.Players)
            .HasForeignKey("MatchId", "TeamId")
            .HasPrincipalKey(
                "MatchId",
                nameof(PlayingTeam.TeamId))
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}