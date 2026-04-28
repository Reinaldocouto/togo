using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class TutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("Tutors");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(120);
        builder.Property(t => t.Document).HasMaxLength(30);
        builder.Property(t => t.Email).HasMaxLength(120);
        builder.Property(t => t.Phone).HasMaxLength(30);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);

        builder.HasIndex(t => t.Document);
    }
}
