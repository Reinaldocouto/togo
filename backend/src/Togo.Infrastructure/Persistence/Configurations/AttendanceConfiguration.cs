using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.AttendanceNumber).IsRequired().HasMaxLength(30);
        builder.Property(a => a.OpenedAt).IsRequired();
        builder.Property(a => a.ClosedAt);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>();
        builder.Property(a => a.Type).IsRequired().HasConversion<string>();

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.AttendanceNumber).IsUnique();
        builder.HasIndex(a => a.OpenedAt);
    }
}
