using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.IsSent)
            .IsRequired();

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.RelatedStudy)
            .WithMany(ds => ds.Notifications)
            .HasForeignKey(n => n.RelatedStudyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.RelatedStudyId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.IsSent);
        builder.HasIndex(n => n.CreatedAt);

        // Query filters for soft delete
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
