using Kiosco.API.Authorization;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RubrosController : ControllerBase
{
    private readonly IRubroService _rubroService;

    public RubrosController(IRubroService rubroService)
    {
        _rubroService = rubroService;
    }

    // Consulta: accesible a cualquier usuario interno autenticado (RF-010).

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RubroDto>>> GetRubros()
    {
        var rubros = await _rubroService.GetAllAsync();
        return Ok(rubros);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RubroDto>> GetRubro(int id)
    {
        var rubro = await _rubroService.GetByIdAsync(id);
        if (rubro == null)
            return NotFound();

        return Ok(rubro);
    }

    // Escritura: requiere el permiso "rubros.gestionar" (RF-007, RF-009, RNF-034).
    // Solo lo poseen Administrador y Operador Administrativo (semillas de RolPermisoConfiguration).

    [HttpPost]
    [RequirePermission("rubros.gestionar")]
    public async Task<ActionResult<RubroDto>> CreateRubro(RubroCreateDto dto)
    {
        var rubro = await _rubroService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetRubro), new { id = rubro.Id }, rubro);
    }

    [HttpPut("{id}")]
    [RequirePermission("rubros.gestionar")]
    public async Task<IActionResult> UpdateRubro(int id, RubroUpdateDto dto)
    {
        await _rubroService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequirePermission("rubros.gestionar")]
    public async Task<IActionResult> DeleteRubro(int id)
    {
        await _rubroService.DeleteAsync(id);
        return NoContent();
    }
}
