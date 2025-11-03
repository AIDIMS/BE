using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class DicomSeriesConfiguration : IEntityTypeConfiguration<DicomSeries>
{
    public void Configure(EntityTypeBuilder<DicomSeries> builder)
    {
        builder.ToTable("DicomSeries");

        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.StudyId)
            .IsRequired();

        builder.Property(ds => ds.SeriesUid)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ds => ds.OrthancSeriesId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ds => ds.SeriesDescription)
            .HasMaxLength(255);

        builder.Property(ds => ds.SeriesNumber)
            .HasMaxLength(50);

        builder.Property(ds => ds.Modality)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(ds => ds.Study)
            .WithMany(study => study.Series)
            .HasForeignKey(ds => ds.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ds => ds.Instances)
            .WithOne(inst => inst.Series)
            .HasForeignKey(inst => inst.SeriesUid)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ds => ds.SeriesUid).IsUnique();
        builder.HasIndex(ds => ds.OrthancSeriesId).IsUnique();
        builder.HasIndex(ds => ds.StudyId);

        // Query filters for soft delete
        builder.HasQueryFilter(ds => !ds.IsDeleted);
    }
}
