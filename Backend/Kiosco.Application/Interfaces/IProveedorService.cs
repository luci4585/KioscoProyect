using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IProveedorService
{
    Task<IEnumerable<ProveedorDto>> GetAllAsync();
    Task<ProveedorDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProveedorDto>> SearchAsync(string? razonSocial, string? cuit);
    Task<ProveedorDto> CreateAsync(ProveedorCreateDto dto);
    Task UpdateAsync(int id, ProveedorUpdateDto dto);
    Task DeleteAsync(int id);
}
