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
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IValidator<UsuarioUpdateDto> _updateValidator;
    private readonly IValidator<UsuarioRolUpdateDto> _rolValidator;

    public UsuariosController(
        IUsuarioService usuarioService,
        IValidator<UsuarioUpdateDto> updateValidator,
        IValidator<UsuarioRolUpdateDto> rolValidator)
    {
        _usuarioService = usuarioService;
        _updateValidator = updateValidator;
        _rolValidator = rolValidator;
    }

    [HttpGet]
    [RequirePermission("usuarios.gestionar")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        return Ok(await _usuarioService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [RequirePermission("usuarios.gestionar")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpPut("{id:int}")]
    [RequirePermission("usuarios.gestionar")]
    public async Task<ActionResult<UsuarioDto>> UpdateUsuario(int id, UsuarioUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var actualizado = await _usuarioService.UpdateAsync(id, dto);
        return Ok(actualizado);
    }

    [HttpPut("{id:int}/rol")]
    [RequirePermission("roles.gestionar")]
    public async Task<ActionResult<UsuarioDto>> UpdateRolUsuario(int id, UsuarioRolUpdateDto dto)
    {
        var validation = await _rolValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(ToErrorDictionary(validation)));

        var actualizado = await _usuarioService.UpdateRolAsync(id, dto);
        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission("usuarios.gestionar")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        await _usuarioService.DeleteAsync(id);
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
