using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BattingScoreConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<BattingScore> builder)
    {
        builder.ToTable("BattingScores");
        builder.HasKey("InningsId", nameof(BattingScore.PlayerId));
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

        builder.ComplexProperty(
        x => x.Dismissal,
        dismissal =>
        {
            dismissal.Property(x => x.WicketType)
                .HasColumnName("DismissalWicketType")
                .IsRequired(false);

            dismissal.Property(x => x.FielderId)
                .HasColumnName("DismissalFielderId")
                .IsRequired(false);
        });
    }

    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        throw new NotImplementedException();
    }
}