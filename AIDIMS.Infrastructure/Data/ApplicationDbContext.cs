using AIDIMS.Domain.Common;
using AIDIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientVisit> PatientVisits { get; set; }
    public DbSet<ImagingOrder> ImagingOrders { get; set; }
    public DbSet<DicomStudy> DicomStudies { get; set; }
    public DbSet<DicomSeries> DicomSeries { get; set; }
    public DbSet<DicomInstance> DicomInstances { get; set; }
    public DbSet<AiResult> AiResults { get; set; }
    public DbSet<Diagnosis> Diagnoses { get; set; }
    public DbSet<ImageAnnotation> ImageAnnotations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update audit fields
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow.AddHours(7);
                    entry.Entity.Id = Guid.NewGuid();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow.AddHours(7);
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
