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