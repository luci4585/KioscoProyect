using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IRubroRepository : IGenericRepository<Rubro>
{
    Task<Rubro?> GetByNameAsync(string nombre);
}
