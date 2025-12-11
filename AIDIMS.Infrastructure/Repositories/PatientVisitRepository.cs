using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;

namespace AIDIMS.Infrastructure.Repositories;

public class PatientVisitRepository : Repository<PatientVisit>, IPatientVisitRepository
{
    public PatientVisitRepository(ApplicationDbContext context) : base(context)
    {
    }
}
