using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIDIMS.Infrastructure.Configurations;

public class ImagingOrderConfiguration : IEntityTypeConfiguration<ImagingOrder>
{
    public void Configure(EntityTypeBuilder<ImagingOrder> builder)
    {
        builder.ToTable("ImagingOrders");

        builder.HasKey(io => io.Id);

        builder.Property(io => io.VisitId)
            .IsRequired();

        builder.Property(io => io.RequestingDoctorId)
            .IsRequired();

        builder.Property(io => io.ModalityRequested)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(io => io.BodyPartRequested)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(io => io.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(io => io.ReasonForStudy)
            .HasColumnType("text");

        // Relationships
        builder.HasOne(io => io.Visit)
            .WithMany(pv => pv.ImagingOrders)
            .HasForeignKey(io => io.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(io => io.RequestingDoctor)
            .WithMany(u => u.RequestedOrders)
            .HasForeignKey(io => io.RequestingDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(io => io.Studies)
            .WithOne(ds => ds.Order)
            .HasForeignKey(ds => ds.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(io => io.VisitId);
        builder.HasIndex(io => io.RequestingDoctorId);
        builder.HasIndex(io => io.Status);

        // Query filters for soft delete
        builder.HasQueryFilter(io => !io.IsDeleted);
    }
}
