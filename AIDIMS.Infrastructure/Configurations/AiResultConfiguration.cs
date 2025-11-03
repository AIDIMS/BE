using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class AiResultConfiguration : IEntityTypeConfiguration<AiResult>
{
    public void Configure(EntityTypeBuilder<AiResult> builder)
    {
        builder.ToTable("AiResults");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.StudyId)
            .IsRequired();

        builder.Property(ar => ar.ModelVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ar => ar.AnalysisDate)
            .IsRequired();

        builder.Property(ar => ar.Classification)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ar => ar.ConfidenceScore)
            .HasPrecision(5, 4);

        builder.Property(ar => ar.DetailedOutput)
            .HasColumnType("text");

        builder.Property(ar => ar.IsReviewed)
            .IsRequired();

        // Relationships
        builder.HasOne(ar => ar.Study)
            .WithOne(ds => ds.AiResult)
            .HasForeignKey<AiResult>(ar => ar.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ar => ar.StudyId).IsUnique();
        builder.HasIndex(ar => ar.AnalysisDate);
        builder.HasIndex(ar => ar.IsReviewed);

        // Query filters for soft delete
        builder.HasQueryFilter(ar => !ar.IsDeleted);
    }
}
