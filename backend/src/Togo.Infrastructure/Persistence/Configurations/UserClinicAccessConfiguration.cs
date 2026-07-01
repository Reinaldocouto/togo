using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence.Configurations;

public class UserClinicAccessConfiguration : IEntityTypeConfiguration<UserClinicAccess>
{
    public void Configure(EntityTypeBuilder<UserClinicAccess> builder)
    {
        builder.ToTable("UserClinicAccesses");

        builder.HasKey(access => access.Id);
        builder.Property(access => access.Id).ValueGeneratedOnAdd();

        builder.Property(access => access.UserId).IsRequired();
        builder.Property(access => access.ClinicId).IsRequired();
        builder.Property(access => access.IsActive).IsRequired();
        builder.Property(access => access.CreatedAt).IsRequired();
        builder.Property(access => access.UpdatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(access => access.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(access => access.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(access => access.UserId);
        builder.HasIndex(access => access.ClinicId);
        builder.HasIndex(access => new { access.UserId, access.ClinicId }).IsUnique();
    }
}
