using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

/// <summary>
/// Entity configuration for User
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(15);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // Relationships
        builder.HasMany(u => u.AssignedVisits)
            .WithOne(v => v.AssignedDoctor)
            .HasForeignKey(v => v.AssignedDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RequestedOrders)
            .WithOne(o => o.RequestingDoctor)
            .HasForeignKey(o => o.RequestingDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedStudies)
            .WithOne(s => s.AssignedDoctor)
            .HasForeignKey(s => s.AssignedDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters for soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
