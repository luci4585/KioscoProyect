using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllAsync();
    Task<ClienteDto?> GetByIdAsync(int id);
    Task<IEnumerable<ClienteDto>> SearchAsync(string? nombre, string? documento);
    Task<ClienteDto> CreateAsync(ClienteCreateDto dto);
    Task UpdateAsync(int id, ClienteUpdateDto dto);
    Task DeleteAsync(int id);
}
