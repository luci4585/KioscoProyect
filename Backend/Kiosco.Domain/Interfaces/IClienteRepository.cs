using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IClienteRepository : IGenericRepository<Cliente>
{
    Task<IEnumerable<Cliente>> SearchAsync(string? nombre, string? documento);
}
