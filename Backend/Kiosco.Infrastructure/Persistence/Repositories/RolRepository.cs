using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(KioscoDbContext context) : base(context)
    {
    }

    public async Task<Rol?> GetByNombreAsync(string nombre)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Nombre == nombre && r.Activo);
    }
}
