using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BowlingScoreConfiguration : IEntityTypeConfiguration<BowlingScore>
{
    public void Configure(EntityTypeBuilder<BowlingScore> builder)
    {
        builder.ToTable("BowlingScores");

        builder.HasKey("InningsId", nameof(BowlingScore.PlayerId));

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
    }
}