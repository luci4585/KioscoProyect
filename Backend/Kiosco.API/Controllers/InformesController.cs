using Kiosco.API.Authorization;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

/// <summary>
/// Informes y dashboard — Etapa 11 (RoadMap §14; RF-059 a RF-072).
/// Consultas con permiso `informes.consultar`; exportación con
/// `informes.exportar` (RNF-034). Los informes de stock (RF-061) y el
/// historial de cierres (RF-069) se sirven desde los módulos existentes
/// (`/api/Stock`, `/api/Cajas`).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InformesController : ControllerBase
{
    private readonly IInformeService _informeService;

    public InformesController(IInformeService informeService)
    {
        _informeService = informeService;
    }

    /// <summary>Resumen del día + indicadores + cierre diario (RF-064, RF-068, RF-071).</summary>
    [HttpGet("resumen")]
    [RequirePermission("informes.consultar")]
    public async Task<ActionResult<ResumenDiarioDto>> GetResumen([FromQuery] DateTime? fecha)
    {
        return Ok(await _informeService.GetResumenDiarioAsync(fecha ?? DateTime.Today));
    }

    /// <summary>Ventas del período con filtros (RF-059, RF-060).</summary>
    [HttpGet("ventas")]
    [RequirePermission("informes.consultar")]
    public async Task<ActionResult<InformeVentasDto>> GetVentas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? usuarioId,
        [FromQuery] int? clienteId,
        [FromQuery] string? metodoPago)
    {
        var h = hasta ?? DateTime.Today;
        return Ok(await _informeService.GetVentasPeriodoAsync(desde ?? h, h, usuarioId, clienteId, metodoPago));
    }

    /// <summary>Ganancia del período (RF-066, fórmula RF-065).</summary>
    [HttpGet("ganancia")]
    [RequirePermission("informes.consultar")]
    public async Task<ActionResult<InformeGananciaDto>> GetGanancia(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var h = hasta ?? DateTime.Today;
        return Ok(await _informeService.GetGananciaPeriodoAsync(desde ?? h, h));
    }

    /// <summary>Productos más vendidos del período (RF-063).</summary>
    [HttpGet("mas-vendidos")]
    [RequirePermission("informes.consultar")]
    public async Task<ActionResult<IEnumerable<ProductoVendidoDto>>> GetMasVendidos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? top)
    {
        var h = hasta ?? DateTime.Today;
        return Ok(await _informeService.GetMasVendidosAsync(desde ?? h, h, top));
    }

    /// <summary>Movimientos y resultados de caja del período (RF-062).</summary>
    [HttpGet("caja")]
    [RequirePermission("informes.consultar")]
    public async Task<ActionResult<IEnumerable<CajaDto>>> GetCaja(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var h = hasta ?? DateTime.Today;
        return Ok(await _informeService.GetCajaPeriodoAsync(desde ?? h, h));
    }

    /// <summary>
    /// Exportación a Excel (RF-072, RNF-057 a RNF-059). Tipos: ventas,
    /// productos, stock, movimientos-caja, ingresos-mercaderia,
    /// informes-diarios, ganancias. Un tipo no soportado responde 400 vía
    /// el middleware (igual que el resto de las reglas de negocio).
    /// </summary>
    [HttpGet("exportar/{tipo}")]
    [RequirePermission("informes.exportar")]
    public async Task<IActionResult> Exportar(
        string tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var h = hasta ?? DateTime.Today;
        var d = desde ?? h;

        var contenido = await _informeService.ExportarAsync(tipo, d, h);

        var nombre = $"informe_{tipo.ToLowerInvariant()}_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx";
        return File(
            contenido,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre);
    }
}
