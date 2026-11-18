using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync();
    Task<ProductoDto?> GetByIdAsync(int id);

    /// <summary>Búsqueda con filtros (RF-016): nombre, código, rubro, estado, disponibilidad de stock.</summary>
    Task<IEnumerable<ProductoDto>> SearchAsync(
        string? nombre,
        string? codigo,
        int? rubroId,
        bool? activo,
        bool? conStock);

    Task<ProductoDto> CreateAsync(ProductoCreateDto dto);
    Task UpdateAsync(int id, ProductoUpdateDto dto);
    Task DeleteAsync(int id);
}
