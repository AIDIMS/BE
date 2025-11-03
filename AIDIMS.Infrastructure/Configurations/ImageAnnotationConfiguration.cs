using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class ImageAnnotationConfiguration : IEntityTypeConfiguration<ImageAnnotation>
{
    public void Configure(EntityTypeBuilder<ImageAnnotation> builder)
    {
        builder.ToTable("ImageAnnotations");

        builder.HasKey(ia => ia.Id);

        builder.Property(ia => ia.InstanceId)
            .IsRequired();

        builder.Property(ia => ia.AnnotationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ia => ia.AnnotationData)
            .IsRequired()
            .HasColumnType("text");

        // Relationships
        builder.HasOne(ia => ia.Instance)
            .WithMany(di => di.Annotations)
            .HasForeignKey(ia => ia.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ia => ia.InstanceId);
        builder.HasIndex(ia => ia.AnnotationType);

        // Query filters for soft delete
        builder.HasQueryFilter(ia => !ia.IsDeleted);
    }
}
