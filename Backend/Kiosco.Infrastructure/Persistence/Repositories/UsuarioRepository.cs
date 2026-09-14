using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.Activo)
            .ToListAsync();
    }

    public override async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid && u.Activo);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo);
    }
}
