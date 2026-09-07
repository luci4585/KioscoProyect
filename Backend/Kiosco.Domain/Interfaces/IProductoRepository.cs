using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<IEnumerable<Producto>> GetByRubroAsync(int rubroId);
    Task<IEnumerable<Producto>> SearchAsync(string? nombre, string? codigo, int? rubroId);
    Task<IEnumerable<Producto>> GetStockBajoAsync();
}
