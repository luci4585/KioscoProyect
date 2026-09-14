using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface ICajaService
{
    Task<CajaDto?> GetCajaAbiertaAsync();
    Task<CajaDto> AbrirAsync(CajaAperturaDto dto, int usuarioId);
    Task<CajaDto> CerrarAsync(int cajaId, CajaCierreDto dto, int usuarioId);
    Task<CajaDto?> GetWithDetailsAsync(int id);
    Task<MovimientoCajaDto> RegistrarMovimientoAsync(int cajaId, MovimientoCajaCreateDto dto, int usuarioId);
}
