using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.AttendanceId).IsRequired();
        builder.Property(p => p.IssuedAt).IsRequired();
        builder.Property(p => p.Notes).HasColumnType("text");

        builder.HasOne<Attendance>()
            .WithMany()
            .HasForeignKey(p => p.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.AttendanceId);
        builder.HasIndex(p => p.IssuedAt);
    }
}
