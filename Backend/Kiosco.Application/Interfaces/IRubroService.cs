using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IRubroService
{
    Task<IEnumerable<RubroDto>> GetAllAsync();
    Task<RubroDto?> GetByIdAsync(int id);
    Task<RubroDto> CreateAsync(RubroCreateDto dto);
    Task UpdateAsync(int id, RubroUpdateDto dto);
    Task DeleteAsync(int id);
}
