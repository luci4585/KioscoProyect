using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IProveedorRepository : IGenericRepository<Proveedor>
{
    Task<Proveedor?> GetByCuitAsync(string cuit);
}
