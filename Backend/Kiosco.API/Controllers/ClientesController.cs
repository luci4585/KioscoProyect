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
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IValidator<ClienteCreateDto> _createValidator;
    private readonly IValidator<ClienteUpdateDto> _updateValidator;

    public ClientesController(
        IClienteService clienteService,
        IValidator<ClienteCreateDto> createValidator,
        IValidator<ClienteUpdateDto> updateValidator)
    {
        _clienteService = clienteService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // Consulta: accesible a cualquier usuario interno autenticado (RF-028, RF-029).

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
    {
        return Ok(await _clienteService.GetAllAsync());
    }

    /// <summary>Búsqueda con filtros (RF-029): nombre y documento. Sin filtros = todos.</summary>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Search(
        [FromQuery] string? nombre,
        [FromQuery] string? documento)
    {
        return Ok(await _clienteService.SearchAsync(nombre, documento));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetCliente(int id)
    {
        var cliente = await _clienteService.GetByIdAsync(id);
        if (cliente == null)
            return NotFound();

        return Ok(cliente);
    }

    // Escritura: requiere el permiso "clientes.gestionar" (RF-007, RNF-034).
    // Solo lo poseen Administrador y Operador Administrativo (semillas de RolPermisoConfiguration).

    [HttpPost]
    [RequirePermission("clientes.gestionar")]
    public async Task<ActionResult<ClienteDto>> CreateCliente(ClienteCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var cliente = await _clienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    [RequirePermission("clientes.gestionar")]
    public async Task<IActionResult> UpdateCliente(int id, ClienteUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        await _clienteService.UpdateAsync(id, dto);
        return NoContent();
    }

    // Baja lógica (RF-031, RF-073/RF-074): nunca eliminación física.
    [HttpDelete("{id:int}")]
    [RequirePermission("clientes.gestionar")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        await _clienteService.DeleteAsync(id);
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
