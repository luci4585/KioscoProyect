using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(KioscoDbContext context) : base(context)
    {
    }
}
