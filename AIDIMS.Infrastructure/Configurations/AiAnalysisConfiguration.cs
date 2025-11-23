using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
{
    public void Configure(EntityTypeBuilder<AiAnalysis> builder)
    {
        builder.ToTable("AiAnalyses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StudyId)
            .IsRequired();

        builder.Property(a => a.ModelVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.AnalysisDate)
            .IsRequired();

        builder.Property(a => a.PrimaryFinding)
            .HasMaxLength(255);

        builder.Property(a => a.OverallConfidence)
            .HasPrecision(5, 4); // DECIMAL(5,4) - range 0.0000 to 9.9999

        builder.Property(a => a.IsReviewed)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(a => a.Study)
            .WithOne(s => s.AiAnalysis)
            .HasForeignKey<AiAnalysis>(a => a.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Findings)
            .WithOne(f => f.Analysis)
            .HasForeignKey(f => f.AnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.StudyId).IsUnique();
        builder.HasIndex(a => a.AnalysisDate);
        builder.HasIndex(a => a.IsReviewed);

        // Query filters for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
