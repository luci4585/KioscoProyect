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
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IValidator<StockMovimientoDto> _movimientoValidator;

    public StockController(
        IStockService stockService,
        IValidator<StockMovimientoDto> movimientoValidator)
    {
        _stockService = stockService;
        _movimientoValidator = movimientoValidator;
    }

    // Consulta: accesible a cualquier usuario interno autenticado (RF-021, RF-026).

    /// <summary>Cantidad disponible de todos los productos (RF-021).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockDto>>> GetStock()
    {
        return Ok(await _stockService.GetAllAsync());
    }

    /// <summary>Productos con cantidad igual o inferior al mínimo (RF-026 — alertas).</summary>
    [HttpGet("alertas")]
    public async Task<ActionResult<IEnumerable<StockDto>>> GetAlertas()
    {
        return Ok(await _stockService.GetAlertasAsync());
    }

    /// <summary>Stock de un producto específico (RF-021).</summary>
    [HttpGet("producto/{productoId:int}")]
    public async Task<ActionResult<StockDto>> GetStockByProducto(int productoId)
    {
        var stock = await _stockService.GetByProductoAsync(productoId);
        if (stock == null)
            return NotFound();

        return Ok(stock);
    }

    // Movimientos: requieren el permiso "stock.gestionar" (RF-007, RNF-034).
    // Solo lo poseen Administrador y Operador Administrativo (semillas de RolPermisoConfiguration).

    /// <summary>Registra un ingreso y actualiza la cantidad disponible (RF-022).</summary>
    [HttpPost("ingreso")]
    [RequirePermission("stock.gestionar")]
    public async Task<ActionResult<StockDto>> Ingresar(StockMovimientoDto dto)
    {
        var validation = await _movimientoValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        return Ok(await _stockService.IngresarAsync(dto.ProductoId, dto.Cantidad));
    }

    /// <summary>
    /// Registra un egreso y actualiza la cantidad disponible (RF-023).
    /// Si supera el stock disponible responde 400 (RF-077).
    /// </summary>
    [HttpPost("egreso")]
    [RequirePermission("stock.gestionar")]
    public async Task<ActionResult<StockDto>> Egresar(StockMovimientoDto dto)
    {
        var validation = await _movimientoValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        return Ok(await _stockService.EgresarAsync(dto.ProductoId, dto.Cantidad));
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
