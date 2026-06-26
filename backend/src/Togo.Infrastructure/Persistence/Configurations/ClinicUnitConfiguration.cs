using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class ClinicUnitConfiguration : IEntityTypeConfiguration<ClinicUnit>
{
    public void Configure(EntityTypeBuilder<ClinicUnit> builder)
    {
        builder.ToTable("ClinicUnits");

        builder.HasKey(cu => cu.Id);
        builder.Property(cu => cu.Id).ValueGeneratedOnAdd();

        builder.Property(cu => cu.ClinicId).IsRequired();
        builder.Property(cu => cu.Name).IsRequired().HasMaxLength(120);
        builder.Property(cu => cu.IsActive).IsRequired();
        builder.Property(cu => cu.CreatedAt).IsRequired();
        builder.Property(cu => cu.UpdatedAt);

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(cu => cu.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cu => cu.ClinicId);
        builder.HasIndex(cu => new { cu.ClinicId, cu.Name }).IsUnique();
    }
}
