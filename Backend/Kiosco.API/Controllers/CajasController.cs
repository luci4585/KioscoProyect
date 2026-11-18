using FluentValidation;
using Kiosco.API.Authorization;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CajasController : ControllerBase
{
    private readonly ICajaService _cajaService;
    private readonly IValidator<CajaAperturaDto> _aperturaValidator;
    private readonly IValidator<CajaCierreDto> _cierreValidator;
    private readonly IValidator<MovimientoCajaCreateDto> _movimientoValidator;
    private readonly IUsuarioService _usuarioService;

    public CajasController(
        ICajaService cajaService,
        IValidator<CajaAperturaDto> aperturaValidator,
        IValidator<CajaCierreDto> cierreValidator,
        IValidator<MovimientoCajaCreateDto> movimientoValidator,
        IUsuarioService usuarioService)
    {
        _cajaService = cajaService;
        _aperturaValidator = aperturaValidator;
        _cierreValidator = cierreValidator;
        _movimientoValidator = movimientoValidator;
        _usuarioService = usuarioService;
    }

    // ── Consulta (RF-054, RF-055, RF-058): solo autenticación ──────────────

    /// <summary>Historial de cajas: aperturas y cierres anteriores (RF-058).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CajaDto>>> GetCajas()
    {
        return Ok(await _cajaService.GetAllAsync());
    }

    /// <summary>
    /// Caja abierta con su resumen: monto inicial, ingresos, egresos y saldo
    /// (RF-055). Devuelve 200 con body null si no hay caja abierta (un 204
    /// rompería la lectura JSON del frontend).
    /// </summary>
    [HttpGet("abierta")]
    public async Task<ActionResult<CajaDto>> GetCajaAbierta()
    {
        var caja = await _cajaService.GetCajaAbiertaAsync();
        return new JsonResult(caja) { StatusCode = 200 };
    }

    /// <summary>Detalle de una caja con sus movimientos (RF-054, RF-058).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CajaDto>> GetCaja(int id)
    {
        var caja = await _cajaService.GetWithDetailsAsync(id);
        if (caja == null)
            return NotFound();

        return Ok(caja);
    }

    // ── Escritura: permisos sembrados (RF-007, RNF-034) ────────────────────

    /// <summary>Apertura de caja con monto inicial (RF-051).</summary>
    [HttpPost("abrir")]
    [RequirePermission("caja.abrir")]
    public async Task<ActionResult<CajaDto>> AbrirCaja(CajaAperturaDto dto)
    {
        var validation = await _aperturaValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var usuario = await ResolveUsuarioAsync();
        if (usuario == null)
            return Unauthorized();

        var caja = await _cajaService.AbrirAsync(dto, usuario.Id);
        return CreatedAtAction(nameof(GetCaja), new { id = caja.Id }, caja);
    }

    /// <summary>
    /// Cierre de caja: saldo del período y diferencia contra el monto
    /// informado (RF-056, RF-057).
    /// </summary>
    [HttpPost("{id:int}/cerrar")]
    [RequirePermission("caja.cerrar")]
    public async Task<ActionResult<CajaDto>> CerrarCaja(int id, CajaCierreDto dto)
    {
        var validation = await _cierreValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var usuario = await ResolveUsuarioAsync();
        if (usuario == null)
            return Unauthorized();

        return Ok(await _cajaService.CerrarAsync(id, dto, usuario.Id));
    }

    /// <summary>Registro de movimientos de caja: ingresos y egresos (RF-052).</summary>
    [HttpPost("{id:int}/movimientos")]
    [RequirePermission("caja.movimientos")]
    public async Task<ActionResult<MovimientoCajaDto>> RegistrarMovimiento(int id, MovimientoCajaCreateDto dto)
    {
        var validation = await _movimientoValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var usuario = await ResolveUsuarioAsync();
        if (usuario == null)
            return Unauthorized();

        return Ok(await _cajaService.RegistrarMovimientoAsync(id, dto, usuario.Id));
    }

    private async Task<UsuarioDto?> ResolveUsuarioAsync()
    {
        var firebaseUid = User.FindFirst("firebase_uid")?.Value;
        return string.IsNullOrEmpty(firebaseUid)
            ? null
            : await _usuarioService.GetByFirebaseUidAsync(firebaseUid);
    }

    private static Dictionary<string, string[]> ToErrorDictionary(FluentValidation.Results.ValidationResult validation)
    {
        return validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
