using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PlayingTeamPlayerConfiguration
    : IEntityTypeConfiguration<PlayingTeamPlayer>
{
    public void Configure(EntityTypeBuilder<PlayingTeamPlayer> builder)
    {
        builder.ToTable("PlayingTeamPlayers");

        builder.Property<Guid>("MatchId");
        builder.Property<int>("TeamId");

        builder.HasKey(
            "MatchId",
            "TeamId",
            nameof(PlayingTeamPlayer.PlayerId));

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.HasOne<PlayingTeam>()
            .WithMany(x => x.Players)
            .HasForeignKey("MatchId", "TeamId")
            .HasPrincipalKey(
                "MatchId",
                nameof(PlayingTeam.TeamId))
            .OnDelete(DeleteBehavior.Cascade);
    }
}