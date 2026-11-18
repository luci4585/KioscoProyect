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
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;
    private readonly IValidator<ProveedorCreateDto> _createValidator;
    private readonly IValidator<ProveedorUpdateDto> _updateValidator;

    public ProveedoresController(
        IProveedorService proveedorService,
        IValidator<ProveedorCreateDto> createValidator,
        IValidator<ProveedorUpdateDto> updateValidator)
    {
        _proveedorService = proveedorService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // Consulta: accesible a cualquier usuario interno autenticado (RF-033).

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores()
    {
        return Ok(await _proveedorService.GetAllAsync());
    }

    /// <summary>Búsqueda con filtros (RoadMap §9): razón social y CUIT. Sin filtros = todos.</summary>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> Search(
        [FromQuery] string? razonSocial,
        [FromQuery] string? cuit)
    {
        return Ok(await _proveedorService.SearchAsync(razonSocial, cuit));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id);
        if (proveedor == null)
            return NotFound();

        return Ok(proveedor);
    }

    // Escritura: requiere el permiso "proveedores.gestionar" (RF-007, RNF-034).
    // Solo lo poseen Administrador y Operador Administrativo (semillas de RolPermisoConfiguration).

    [HttpPost]
    [RequirePermission("proveedores.gestionar")]
    public async Task<ActionResult<ProveedorDto>> CreateProveedor(ProveedorCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var proveedor = await _proveedorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:int}")]
    [RequirePermission("proveedores.gestionar")]
    public async Task<IActionResult> UpdateProveedor(int id, ProveedorUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        await _proveedorService.UpdateAsync(id, dto);
        return NoContent();
    }

    // Baja lógica (RF-035, RF-073/RF-074): nunca eliminación física.
    [HttpDelete("{id:int}")]
    [RequirePermission("proveedores.gestionar")]
    public async Task<IActionResult> DeleteProveedor(int id)
    {
        await _proveedorService.DeleteAsync(id);
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
