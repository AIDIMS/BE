using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class DicomStudyConfiguration : IEntityTypeConfiguration<DicomStudy>
{
    public void Configure(EntityTypeBuilder<DicomStudy> builder)
    {
        builder.ToTable("DicomStudies");

        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.OrderId)
            .IsRequired();

        builder.Property(ds => ds.PatientId)
            .IsRequired();

        builder.Property(ds => ds.StudyUid)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ds => ds.OrthancStudyId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ds => ds.StudyDescription)
            .HasMaxLength(255);

        builder.Property(ds => ds.AccessionNumber)
            .HasMaxLength(100);

        builder.Property(ds => ds.Modality)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ds => ds.StudyDate)
            .IsRequired();

        builder.Property(ds => ds.AssignedDoctorId)
            .IsRequired();

        // Relationships
        builder.HasOne(ds => ds.Order)
            .WithMany(io => io.Studies)
            .HasForeignKey(ds => ds.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ds => ds.Patient)
            .WithMany(p => p.Studies)
            .HasForeignKey(ds => ds.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ds => ds.AssignedDoctor)
            .WithMany(u => u.AssignedStudies)
            .HasForeignKey(ds => ds.AssignedDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ds => ds.Series)
            .WithOne(ser => ser.Study)
            .HasForeignKey(ser => ser.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ds => ds.AiAnalysis)
            .WithOne(a => a.Study)
            .HasForeignKey<AiAnalysis>(a => a.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ds => ds.Diagnosis)
            .WithOne(d => d.Study)
            .HasForeignKey<Diagnosis>(d => d.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ds => ds.Notifications)
            .WithOne(n => n.RelatedStudy)
            .HasForeignKey(n => n.RelatedStudyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(ds => ds.StudyUid).IsUnique();
        builder.HasIndex(ds => ds.OrthancStudyId).IsUnique();
        builder.HasIndex(ds => ds.OrderId);
        builder.HasIndex(ds => ds.PatientId);
        builder.HasIndex(ds => ds.AssignedDoctorId);
        builder.HasIndex(ds => ds.StudyDate);

        // Query filters for soft delete
        builder.HasQueryFilter(ds => !ds.IsDeleted);
    }
}
