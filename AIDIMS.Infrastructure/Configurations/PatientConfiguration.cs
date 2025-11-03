using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(15);

        builder.Property(p => p.Address)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(p => p.PatientCode)
            .IsUnique();

        // Relationships
        builder.HasMany(p => p.Visits)
            .WithOne(v => v.Patient)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Studies)
            .WithOne(s => s.Patient)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filters for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
