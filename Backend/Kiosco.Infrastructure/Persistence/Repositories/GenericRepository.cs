using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly KioscoDbContext _context;

    public GenericRepository(KioscoDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.FechaCreacion = DateTime.UtcNow;
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.FechaModificacion = DateTime.UtcNow;
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null)
        {
            entity.Activo = false;
            entity.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _context.Set<T>().AnyAsync(e => e.Id == id);
    }
}
