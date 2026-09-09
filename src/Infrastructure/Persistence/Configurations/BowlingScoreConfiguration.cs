using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BowlingScoreConfiguration : IEntityTypeConfiguration<BowlingScore>
{
    public void Configure(EntityTypeBuilder<BowlingScore> builder)
    {
        builder.ToTable("BowlingScores");

        // Shadow property for the foreign key reference
        builder.Property<Guid>("InningsId")
            .IsRequired();

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.RunsConceded)
            .IsRequired();

        builder.Property(x => x.Balls)
            .IsRequired();

        builder.Property(x => x.Maidens)
            .IsRequired();

        builder.Property(x => x.TotalWickets)
            .IsRequired();

        // Composite primary key (Player per Innings)
        builder.HasKey("InningsId", nameof(BowlingScore.PlayerId));

        // 🌟 Sever the tracking loop by using an index instead of an upward .HasOne navigation
        builder.HasIndex("InningsId");
    }
}
