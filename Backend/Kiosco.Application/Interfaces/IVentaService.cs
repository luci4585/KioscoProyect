using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<VentaDto>> GetAllAsync();
    Task<VentaDto?> GetByIdAsync(int id);
    Task<VentaDto> CreateAsync(VentaCreateDto dto, int usuarioId);
    Task AnularAsync(int id);
}
