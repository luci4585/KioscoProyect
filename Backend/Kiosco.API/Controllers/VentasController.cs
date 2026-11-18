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
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IValidator<VentaCreateDto> _createValidator;
    private readonly IUsuarioService _usuarioService;

    public VentasController(
        IVentaService ventaService,
        IValidator<VentaCreateDto> createValidator,
        IUsuarioService usuarioService)
    {
        _ventaService = ventaService;
        _createValidator = createValidator;
        _usuarioService = usuarioService;
    }

    // ── Consulta (RF-048, RF-049) ───────────────────────────────────────────

    [HttpGet]
    [RequirePermission("ventas.consultar")]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
    {
        return Ok(await _ventaService.GetAllAsync());
    }

    /// <summary>Detalle de una venta: productos, cantidades, precios y subtotales (RF-049).</summary>
    [HttpGet("{id:int}")]
    [RequirePermission("ventas.consultar")]
    public async Task<ActionResult<VentaDto>> GetVenta(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
            return NotFound();

        return Ok(venta);
    }

    // ── Registro (RF-040 a RF-047) ──────────────────────────────────────────

    [HttpPost]
    [RequirePermission("ventas.crear")]
    public async Task<ActionResult<VentaDto>> CreateVenta(VentaCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        // RF-046: la venta registra el usuario interno del vendedor autenticado.
        var firebaseUid = User.FindFirst("firebase_uid")?.Value;
        var usuario = string.IsNullOrEmpty(firebaseUid)
            ? null
            : await _usuarioService.GetByFirebaseUidAsync(firebaseUid);
        if (usuario == null)
            return Unauthorized();

        var venta = await _ventaService.CreateAsync(dto, usuario.Id);
        return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, venta);
    }

    // ── Anulación (RF-050) ──────────────────────────────────────────────────
    // Conserva la información histórica y restituye el stock; el ajuste de
    // movimientos de caja se implementa en la Etapa 10 (RF-050 "cuando corresponda").

    [HttpDelete("{id:int}")]
    [RequirePermission("ventas.anular")]
    public async Task<IActionResult> AnularVenta(int id)
    {
        // RF-050/RF-052: el responsable del ajuste es el usuario autenticado.
        var firebaseUid = User.FindFirst("firebase_uid")?.Value;
        var usuario = string.IsNullOrEmpty(firebaseUid)
            ? null
            : await _usuarioService.GetByFirebaseUidAsync(firebaseUid);
        if (usuario == null)
            return Unauthorized();

        await _ventaService.AnularAsync(id, usuario.Id);
        return NoContent();
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
