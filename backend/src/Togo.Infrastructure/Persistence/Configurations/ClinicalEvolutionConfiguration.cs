using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class ClinicalEvolutionConfiguration : IEntityTypeConfiguration<ClinicalEvolution>
{
    public void Configure(EntityTypeBuilder<ClinicalEvolution> builder)
    {
        builder.ToTable("ClinicalEvolutions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.AttendanceId).IsRequired();
        builder.Property(e => e.RegisteredAt).IsRequired();
        builder.Property(e => e.Type).IsRequired().HasConversion<string>();
        builder.Property(e => e.Text).IsRequired().HasColumnType("text");
        builder.Property(e => e.CreatedByUserId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedByUserId).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasOne<Attendance>()
            .WithMany()
            .HasForeignKey(e => e.AttendanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AttendanceId);
        builder.HasIndex(e => e.RegisteredAt);
    }
}
