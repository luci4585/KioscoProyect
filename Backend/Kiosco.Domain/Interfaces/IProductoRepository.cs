using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<IEnumerable<Producto>> GetByRubroAsync(int rubroId);

    /// <summary>
    /// Búsqueda con filtros (RF-016): nombre, código, rubro, estado y
    /// disponibilidad de stock.
    /// <list type="bullet">
    /// <item><c>activo</c>: null = solo activos (consulta general RF-015); true/false filtra explícitamente.</item>
    /// <item><c>conStock</c>: null = sin filtro; true = con disponibilidad; false = sin disponibilidad.</item>
    /// </list>
    /// </summary>
    Task<IEnumerable<Producto>> SearchAsync(
        string? nombre,
        string? codigo,
        int? rubroId,
        bool? activo,
        bool? conStock);

    Task<IEnumerable<Producto>> GetStockBajoAsync();

    /// <summary>
    /// Busca por código <b>sin filtrar por estado</b>: el índice único de Codigo
    /// aplica a todos los registros, incluidos los dados de baja (RF-076).
    /// </summary>
    Task<Producto?> GetByCodigoAsync(string codigo);
}
