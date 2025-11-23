using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class AiFindingConfiguration : IEntityTypeConfiguration<AiFinding>
{
    public void Configure(EntityTypeBuilder<AiFinding> builder)
    {
        builder.ToTable("AiFindings");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.AnalysisId)
            .IsRequired();

        builder.Property(f => f.Label)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.ConfidenceScore)
            .IsRequired()
            .HasPrecision(5, 4); // DECIMAL(5,4) - range 0.0000 to 9.9999

        // Bounding Box coordinates
        builder.Property(f => f.XMin)
            .HasPrecision(10, 5);

        builder.Property(f => f.YMin)
            .HasPrecision(10, 5);

        builder.Property(f => f.XMax)
            .HasPrecision(10, 5);

        builder.Property(f => f.YMax)
            .HasPrecision(10, 5);

        // Relationships
        builder.HasOne(f => f.Analysis)
            .WithMany(a => a.Findings)
            .HasForeignKey(f => f.AnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(f => f.AnalysisId);
        builder.HasIndex(f => f.Label);
        builder.HasIndex(f => f.ConfidenceScore);
    }
}
