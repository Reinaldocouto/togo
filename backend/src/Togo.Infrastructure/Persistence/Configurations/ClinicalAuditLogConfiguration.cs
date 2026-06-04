using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class ClinicalAuditLogConfiguration : IEntityTypeConfiguration<ClinicalAuditLog>
{
    public void Configure(EntityTypeBuilder<ClinicalAuditLog> builder)
    {
        builder.ToTable("ClinicalAuditLogs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.UserProfile).HasMaxLength(50);
        builder.Property(a => a.OccurredAt).IsRequired();
        builder.Property(a => a.MetadataJson).HasColumnType("longtext");

        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.OccurredAt);
        builder.HasIndex(a => a.UserId);
    }
}
