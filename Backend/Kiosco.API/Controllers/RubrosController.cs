using Kiosco.Application.DTOs;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RubrosController : ControllerBase
{
    private readonly KioscoDbContext _context;

    public RubrosController(KioscoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RubroDto>>> GetRubros()
    {
        var rubros = await _context.Rubros
            .Where(r => r.Activo)
            .Select(r => new RubroDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Activo = r.Activo
            })
            .ToListAsync();

        return Ok(rubros);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RubroDto>> GetRubro(int id)
    {
        var rubro = await _context.Rubros.FindAsync(id);

        if (rubro == null)
            return NotFound();

        return Ok(new RubroDto
        {
            Id = rubro.Id,
            Nombre = rubro.Nombre,
            Descripcion = rubro.Descripcion,
            Activo = rubro.Activo
        });
    }

    [HttpPost]
    public async Task<ActionResult<RubroDto>> CreateRubro(RubroCreateDto dto)
    {
        var rubro = new Kiosco.Domain.Entities.Rubro
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRubro), new { id = rubro.Id }, new RubroDto
        {
            Id = rubro.Id,
            Nombre = rubro.Nombre,
            Descripcion = rubro.Descripcion,
            Activo = rubro.Activo
        });
    }
}
