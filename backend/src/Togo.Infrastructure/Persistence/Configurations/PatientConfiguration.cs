using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.ClinicId).IsRequired();
        builder.Property(p => p.Type).IsRequired().HasConversion<string>();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Property(p => p.BirthDate);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => new { p.ClinicId, p.Name });
        builder.HasIndex(p => p.Status);
    }
}
