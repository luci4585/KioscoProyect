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

    // Sin filtro de estado: el llamador decide (ver IUsuarioRepository).
    public async Task<Usuario?> GetByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
    }

    // Sin filtro de estado: el índice único de Email aplica a todos los registros.
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    // Sin filtro de estado: el índice único de NombreUsuario aplica a todos los registros.
    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<Usuario?> GetWithPermissionsAsync(string firebaseUid)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
    }
}
