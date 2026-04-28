using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(p => p.PatientId);

        builder.Property(p => p.PatientId).ValueGeneratedNever();
        builder.Property(p => p.TutorId).IsRequired();
        builder.Property(p => p.Species).IsRequired().HasMaxLength(40);
        builder.Property(p => p.Breed).HasMaxLength(60);
        builder.Property(p => p.Sex).IsRequired().HasConversion<string>();
        builder.Property(p => p.WeightKg).HasPrecision(6, 2);
        builder.Property(p => p.Microchip).HasMaxLength(40);

        builder.HasOne<Patient>()
            .WithOne()
            .HasForeignKey<Pet>(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TutorId);
        builder.HasIndex(p => p.Microchip);
    }
}
