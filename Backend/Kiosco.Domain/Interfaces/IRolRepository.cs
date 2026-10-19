using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IRolRepository : IGenericRepository<Rol>
{
    Task<Rol?> GetByNombreAsync(string nombre);
}
