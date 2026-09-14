using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(KioscoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Cliente>> SearchAsync(string? nombre, string? documento)
    {
        var query = _context.Clientes.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(c => c.Nombre.Contains(nombre) || c.Apellido.Contains(nombre));

        if (!string.IsNullOrWhiteSpace(documento))
            query = query.Where(c => c.Documento != null && c.Documento.Contains(documento));

        return await query.ToListAsync();
    }
}
