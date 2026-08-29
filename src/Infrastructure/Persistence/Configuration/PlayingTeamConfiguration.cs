using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PlayingTeamConfiguration
    : IEntityTypeConfiguration<PlayingTeam>
{
    public void Configure(EntityTypeBuilder<PlayingTeam> builder)
    {
        builder.ToTable("PlayingTeams");

        builder.Property<Guid>("MatchId");

        builder.HasKey(
            "MatchId",
            nameof(PlayingTeam.TeamId));

        builder.Property(x => x.TeamId)
            .IsRequired();

        builder.Navigation(x => x.Players)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}