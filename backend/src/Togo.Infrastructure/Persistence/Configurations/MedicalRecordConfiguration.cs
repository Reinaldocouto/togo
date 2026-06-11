using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.PatientId).IsRequired();
        builder.Property(m => m.GeneralNotes).HasColumnType("text");
        builder.Property(m => m.FlagsJson).HasColumnType("longtext");
        builder.Property(m => m.CreatedByUserId).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedByUserId).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();
        builder.Property(m => m.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.DeletedAt);
        builder.Property(m => m.DeletedByUserId);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.PatientId).IsUnique().HasDatabaseName("IX_MedicalRecords_PatientId");
    }
}
