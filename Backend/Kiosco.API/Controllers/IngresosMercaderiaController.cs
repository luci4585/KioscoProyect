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
public class IngresosMercaderiaController : ControllerBase
{
    private readonly IIngresoMercaderiaService _ingresoService;
    private readonly IValidator<IngresoMercaderiaCreateDto> _createValidator;

    public IngresosMercaderiaController(
        IIngresoMercaderiaService ingresoService,
        IValidator<IngresoMercaderiaCreateDto> createValidator)
    {
        _ingresoService = ingresoService;
        _createValidator = createValidator;
    }

    // Consulta: accesible a cualquier usuario interno autenticado.

    /// <summary>Lista de ingresos registrados, ordenada por fecha descendente.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IngresoMercaderiaDto>>> GetIngresos()
    {
        return Ok(await _ingresoService.GetAllAsync());
    }

    /// <summary>Ingreso con sus detalles (productos, cantidades y costos).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<IngresoMercaderiaDto>> GetIngreso(int id)
    {
        var ingreso = await _ingresoService.GetByIdAsync(id);
        if (ingreso == null)
            return NotFound();

        return Ok(ingreso);
    }

    // Escritura: requiere "stock.gestionar" (RF-007, RNF-034). El ingreso de
    // mercadería es un movimiento de stock (RF-038) operado por Administrador y
    // Operador Administrativo (semillas de RolPermisoConfiguration): el permiso
    // sembrado existente evita crear permisos nuevos que exigirían una migración.

    /// <summary>
    /// Crea un ingreso asociado a su proveedor (RF-036, RF-037), registra
    /// cantidades y costos (RF-039) e incrementa el stock automáticamente (RF-038).
    /// </summary>
    [HttpPost]
    [RequirePermission("stock.gestionar")]
    public async Task<ActionResult<IngresoMercaderiaDto>> CreateIngreso(IngresoMercaderiaCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var ingreso = await _ingresoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetIngreso), new { id = ingreso.Id }, ingreso);
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
