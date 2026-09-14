using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class CajaService : ICajaService
{
    private readonly ICajaRepository _cajaRepository;
    private readonly IMovimientoCajaRepository _movimientoRepository;

    public CajaService(ICajaRepository cajaRepository, IMovimientoCajaRepository movimientoRepository)
    {
        _cajaRepository = cajaRepository;
        _movimientoRepository = movimientoRepository;
    }

    public async Task<CajaDto?> GetCajaAbiertaAsync()
    {
        var caja = await _cajaRepository.GetCajaAbiertaAsync();
        if (caja == null) return null;

        caja = await _cajaRepository.GetWithMovimientosAsync(caja.Id);
        return caja?.ToDto();
    }

    public async Task<CajaDto> AbrirAsync(CajaAperturaDto dto, int usuarioId)
    {
        var cajaAbierta = await _cajaRepository.GetCajaAbiertaAsync();
        if (cajaAbierta != null)
            throw new BusinessRuleException("Ya existe una caja abierta. Debe cerrarla antes de abrir una nueva.");

        var caja = new Caja
        {
            FechaApertura = DateTime.UtcNow,
            MontoInicial = dto.MontoInicial,
            Abierta = true,
            UsuarioAperturaId = usuarioId
        };

        var created = await _cajaRepository.CreateAsync(caja);
        return created.ToDto();
    }

    public async Task<CajaDto> CerrarAsync(int cajaId, CajaCierreDto dto, int usuarioId)
    {
        var caja = await _cajaRepository.GetWithMovimientosAsync(cajaId)
            ?? throw new EntityNotFoundException(nameof(Caja), cajaId);

        if (!caja.Abierta)
            throw new BusinessRuleException("La caja ya está cerrada.");

        caja.FechaCierre = DateTime.UtcNow;
        caja.MontoFinal = dto.MontoFinalInformado;
        caja.UsuarioCierreId = usuarioId;
        caja.Abierta = false;
        var saldoCalculado = caja.MontoInicial
            + (caja.Movimientos?.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto) ?? 0)
            - (caja.Movimientos?.Where(m => m.TipoMovimiento == "Egreso").Sum(m => m.Monto) ?? 0);
        caja.Diferencia = dto.MontoFinalInformado - saldoCalculado;

        await _cajaRepository.UpdateAsync(caja);
        return caja.ToDto();
    }

    public async Task<CajaDto?> GetWithDetailsAsync(int id)
    {
        var caja = await _cajaRepository.GetWithMovimientosAsync(id);
        return caja?.ToDto();
    }

    public async Task<MovimientoCajaDto> RegistrarMovimientoAsync(int cajaId, MovimientoCajaCreateDto dto, int usuarioId)
    {
        var caja = await _cajaRepository.GetCajaAbiertaAsync()
            ?? throw new BusinessRuleException("No hay una caja abierta.");

        if (caja.Id != cajaId)
            throw new BusinessRuleException("La caja indicada no es la caja abierta actual.");

        var movimiento = new MovimientoCaja
        {
            CajaId = cajaId,
            Fecha = DateTime.UtcNow,
            TipoMovimiento = dto.TipoMovimiento,
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            UsuarioId = usuarioId
        };

        var created = await _movimientoRepository.CreateAsync(movimiento);
        return created.ToDto();
    }
}
