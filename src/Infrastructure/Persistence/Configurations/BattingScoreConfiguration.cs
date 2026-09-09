using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BattingScoreConfiguration : IEntityTypeConfiguration<BattingScore>
{
    public void Configure(EntityTypeBuilder<BattingScore> builder)
    {
        builder.ToTable("BattingScores");

        // Shadow property for the foreign key reference
        builder.Property<Guid>("InningsId")
            .IsRequired();

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.Runs)
            .IsRequired();

        builder.Property(x => x.Balls)
            .IsRequired();

        builder.Property(x => x.Fours)
            .IsRequired();

        builder.Property(x => x.Sixes)
            .IsRequired();

        // Composite primary key (Player per Innings)
        builder.HasKey("InningsId", nameof(BattingScore.PlayerId));

        // Complex value object property mapping for wickets/dismissals
        builder.ComplexProperty(
            x => x.Dismissal,
            dismissal =>
            {
                dismissal.Property(x => x.WicketType)
                    .HasColumnName("DismissalWicketType")
                    .IsRequired();

                dismissal.Property(x => x.FielderId)
                    .HasColumnName("DismissalFielderId")
                    .IsRequired(false);
            });

        // 🌟 Sever the tracking loop by using an index instead of an upward .HasOne navigation
        builder.HasIndex("InningsId");
    }
}
