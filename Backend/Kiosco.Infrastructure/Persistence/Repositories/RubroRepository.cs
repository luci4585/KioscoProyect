using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class RubroRepository : GenericRepository<Rubro>, IRubroRepository
{
    public RubroRepository(KioscoDbContext context) : base(context)
    {
    }

    public async Task<Rubro?> GetByNameAsync(string nombre)
    {
        return await _context.Rubros
            .FirstOrDefaultAsync(r => r.Nombre == nombre && r.Activo);
    }
}
