using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("Diagnoses");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.StudyId)
            .IsRequired();

        builder.Property(d => d.FinalDiagnosis)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(d => d.ReportStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(d => d.Study)
            .WithOne(ds => ds.Diagnosis)
            .HasForeignKey<Diagnosis>(d => d.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.StudyId).IsUnique();
        builder.HasIndex(d => d.ReportStatus);

        // Query filters for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
