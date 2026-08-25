using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.BallNumberInOver)
            .IsRequired();

        builder.Property(x => x.StrikerId)
            .IsRequired();

        builder.Property(x => x.NonStrikerId)
            .IsRequired();

        builder.Property(x => x.BowlerId)
            .IsRequired();

        builder.Property(x => x.BatterRuns)
            .IsRequired();

        builder.Property(x => x.TotalRuns)
            .IsRequired();

        builder.Property(x => x.ExtraType)
            .IsRequired();

        builder.Property(x => x.Dismissal)
            .IsRequired();

        builder.Property(x => x.DismissedPlayerId)
            .IsRequired(false);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.HasIndex("OverId", nameof(Delivery.SequenceNumber))
            .IsUnique();

        builder.HasIndex("OverId", nameof(Delivery.BallNumberInOver));
    }
}