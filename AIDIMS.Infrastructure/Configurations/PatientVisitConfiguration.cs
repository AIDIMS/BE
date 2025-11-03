using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class PatientVisitConfiguration : IEntityTypeConfiguration<PatientVisit>
{
    public void Configure(EntityTypeBuilder<PatientVisit> builder)
    {
        builder.ToTable("PatientVisits");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.PatientId)
            .IsRequired();

        builder.Property(pv => pv.AssignedDoctorId)
            .IsRequired();

        builder.Property(pv => pv.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(pv => pv.Symptoms)
            .HasColumnType("text");

        // Relationships
        builder.HasOne(pv => pv.Patient)
            .WithMany(p => p.Visits)
            .HasForeignKey(pv => pv.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pv => pv.AssignedDoctor)
            .WithMany(u => u.AssignedVisits)
            .HasForeignKey(pv => pv.AssignedDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pv => pv.ImagingOrders)
            .WithOne(io => io.Visit)
            .HasForeignKey(io => io.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pv => pv.PatientId);
        builder.HasIndex(pv => pv.AssignedDoctorId);
        builder.HasIndex(pv => pv.Status);

        // Query filters for soft delete
        builder.HasQueryFilter(pv => !pv.IsDeleted);
    }
}
