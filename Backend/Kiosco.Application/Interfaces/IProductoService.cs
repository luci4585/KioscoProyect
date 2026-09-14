using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync();
    Task<ProductoDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductoDto>> SearchAsync(string? nombre, string? codigo, int? rubroId);
    Task<ProductoDto> CreateAsync(ProductoCreateDto dto);
    Task UpdateAsync(int id, ProductoUpdateDto dto);
    Task DeleteAsync(int id);
}
