using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IIngresoMercaderiaService
{
    Task<IEnumerable<IngresoMercaderiaDto>> GetAllAsync();
    Task<IngresoMercaderiaDto?> GetByIdAsync(int id);
    Task<IngresoMercaderiaDto> CreateAsync(IngresoMercaderiaCreateDto dto);
}
