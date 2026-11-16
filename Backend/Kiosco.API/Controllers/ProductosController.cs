using FluentValidation;
using Kiosco.API.Authorization;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IValidator<ProductoCreateDto> _createValidator;
    private readonly IValidator<ProductoUpdateDto> _updateValidator;

    public ProductosController(
        IProductoService productoService,
        IValidator<ProductoCreateDto> createValidator,
        IValidator<ProductoUpdateDto> updateValidator)
    {
        _productoService = productoService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── Consulta (RF-015, RF-016) ───────────────────────────────────────────

    [HttpGet]
    [RequirePermission("productos.consultar")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
    {
        return Ok(await _productoService.GetAllAsync());
    }

    /// <summary>
    /// Búsqueda con filtros (RF-016): nombre, código, rubro, estado y
    /// disponibilidad de stock. Parámetros opcionales; sin filtros = todos.
    /// </summary>
    [HttpGet("buscar")]
    [RequirePermission("productos.consultar")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> Search(
        [FromQuery] string? nombre,
        [FromQuery] string? codigo,
        [FromQuery] int? rubroId,
        [FromQuery] bool? activo,
        [FromQuery] bool? conStock)
    {
        var productos = await _productoService.SearchAsync(nombre, codigo, rubroId, activo, conStock);
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    [RequirePermission("productos.consultar")]
    public async Task<ActionResult<ProductoDto>> GetProducto(int id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto == null)
            return NotFound();

        return Ok(producto);
    }

    // ── Escritura (RF-013, RF-017, RF-018, RF-019) ─────────────────────────

    [HttpPost]
    [RequirePermission("productos.crear")]
    public async Task<ActionResult<ProductoDto>> CreateProducto(ProductoCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var producto = await _productoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    [RequirePermission("productos.editar")]
    public async Task<IActionResult> UpdateProducto(int id, ProductoUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        await _productoService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission("productos.eliminar")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        await _productoService.DeleteAsync(id);
        return NoContent();
    }

    // ── Imágenes (RF-020, RNF-035 a RNF-038) ───────────────────────────────

    [HttpPost("{id:int}/imagen")]
    [RequirePermission("productos.editar")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ProductoDto>> UploadImagen(int id, IFormFile imagen)
    {
        if (imagen == null || imagen.Length == 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["imagen"] = new[] { "La imagen es requerida." }
            }));

        using var stream = imagen.OpenReadStream();
        var producto = await _productoService.UploadImageAsync(
            id, stream, imagen.FileName, imagen.ContentType);

        return Ok(producto);
    }

    [HttpDelete("{id:int}/imagen")]
    [RequirePermission("productos.editar")]
    public async Task<ActionResult<ProductoDto>> DeleteImagen(int id)
    {
        var producto = await _productoService.RemoveImageAsync(id);
        return Ok(producto);
    }

    // ── Utilidades ──────────────────────────────────────────────────────────

    private static Dictionary<string, string[]> ToErrorDictionary(FluentValidation.Results.ValidationResult validation)
    {
        return validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
