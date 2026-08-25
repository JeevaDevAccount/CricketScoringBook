using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OverConfiguration : IEntityTypeConfiguration<Over>
{
    public void Configure(EntityTypeBuilder<Over> builder)
    {
        builder.ToTable("Overs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OverNumber)
            .IsRequired();

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.IsEditable)
            .IsRequired();

        builder.HasMany(x => x.Deliveries)
            .WithOne()
            .HasForeignKey("OverId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex("InningsId", nameof(Over.OverNumber))
            .IsUnique();
    }
}