using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.PrescriptionId).IsRequired();
        builder.Property(i => i.ProductId);
        builder.Property(i => i.Quantity).IsRequired().HasPrecision(12, 3);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Dosage).IsRequired().HasMaxLength(200);
        builder.Property(i => i.DurationDays);

        builder.HasOne<Prescription>()
            .WithMany()
            .HasForeignKey(i => i.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.PrescriptionId);
        builder.HasIndex(i => i.ProductId);
    }
}
