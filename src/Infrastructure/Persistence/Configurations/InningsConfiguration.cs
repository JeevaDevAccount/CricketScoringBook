using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InningsConfiguration
    : IEntityTypeConfiguration<Innings>
{
    public void Configure(EntityTypeBuilder<Innings> builder)
    {
        builder.ToTable("Innings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.InningsNumber).IsRequired();
        builder.Property(x => x.BattingTeamId).IsRequired();
        builder.Property(x => x.BowlingTeamId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.MaxOvers).IsRequired();
        builder.Property(x => x.TargetRuns).IsRequired(false);
        builder.Property(x => x.TotalRuns).IsRequired();
        builder.Property(x => x.Wickets).IsRequired();
        builder.Property(x => x.TotalBalls).IsRequired();
        builder.Property(x => x.CurrentStrikerId).IsRequired();
        builder.Property(x => x.CurrentNonStrikerId).IsRequired();
        builder.Property(x => x.CurrentBowlerId).IsRequired();
        builder.Property(x => x.IsCompleted).IsRequired();
        builder.Property<Guid>("MatchId").IsRequired();

        builder.HasOne<Match>()
            .WithMany(x => x.Innings)
            .HasForeignKey("MatchId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Overs)
            .WithOne()
            .HasForeignKey("InningsId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(
                "MatchId",
                nameof(Innings.InningsNumber))
            .IsUnique();
    }
}