using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class DicomInstanceConfiguration : IEntityTypeConfiguration<DicomInstance>
{
    public void Configure(EntityTypeBuilder<DicomInstance> builder)
    {
        builder.ToTable("DicomInstances");

        builder.HasKey(di => di.Id);

        builder.Property(di => di.SeriesUid)
            .IsRequired();

        builder.Property(di => di.SopInstanceUid)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(di => di.OrthancInstanceId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(di => di.InstanceNumber)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(di => di.Series)
            .WithMany(ds => ds.Instances)
            .HasForeignKey(di => di.SeriesUid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(di => di.Annotations)
            .WithOne(ia => ia.Instance)
            .HasForeignKey(ia => ia.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(di => di.SopInstanceUid).IsUnique();
        builder.HasIndex(di => di.OrthancInstanceId).IsUnique();
        builder.HasIndex(di => di.SeriesUid);

        // Query filters for soft delete
        builder.HasQueryFilter(di => !di.IsDeleted);
    }
}
