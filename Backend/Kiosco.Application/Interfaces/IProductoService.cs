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

    /// <summary>
    /// Carga una imagen para el producto, la asocia en la BD y elimina la
    /// anterior si existía (RF-020: carga, asociación y reemplazo).
    /// </summary>
    Task<ProductoDto> UploadImageAsync(
        int id,
        Stream content,
        string fileName,
        string contentType);

    /// <summary>
    /// Elimina la imagen del almacenamiento y de la referencia del producto (RF-020).
    /// </summary>
    Task<ProductoDto> RemoveImageAsync(int id);
}
