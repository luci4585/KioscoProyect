using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class MovimientoCajaRepository : GenericRepository<MovimientoCaja>, IMovimientoCajaRepository
{
    public MovimientoCajaRepository(KioscoDbContext context) : base(context)
    {
    }
}
