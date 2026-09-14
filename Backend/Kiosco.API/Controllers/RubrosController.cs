using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RubrosController : ControllerBase
{
    private readonly IRubroService _rubroService;

    public RubrosController(IRubroService rubroService)
    {
        _rubroService = rubroService;
    }

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

    [HttpPost]
    public async Task<ActionResult<RubroDto>> CreateRubro(RubroCreateDto dto)
    {
        var rubro = await _rubroService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetRubro), new { id = rubro.Id }, rubro);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRubro(int id, RubroUpdateDto dto)
    {
        await _rubroService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRubro(int id)
    {
        await _rubroService.DeleteAsync(id);
        return NoContent();
    }
}
