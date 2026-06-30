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

        builder.Property(p => p.ClinicId).IsRequired();
        builder.Property(p => p.AttendanceId).IsRequired();
        builder.Property(p => p.IssuedAt).IsRequired();
        builder.Property(p => p.Notes).HasColumnType("text");

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Attendance>()
            .WithMany()
            .HasForeignKey(p => p.AttendanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ClinicId).HasDatabaseName("IX_Prescriptions_ClinicId");
        builder.HasIndex(p => new { p.ClinicId, p.AttendanceId }).HasDatabaseName("IX_Prescriptions_ClinicId_AttendanceId");
        builder.HasIndex(p => new { p.ClinicId, p.IssuedAt }).HasDatabaseName("IX_Prescriptions_ClinicId_IssuedAt");
        builder.HasIndex(p => p.AttendanceId);
        builder.HasIndex(p => p.IssuedAt);
    }
}
