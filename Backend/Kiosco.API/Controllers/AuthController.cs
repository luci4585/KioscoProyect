using System.Security.Claims;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;

    public AuthController(
        IUsuarioService usuarioService,
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository)
    {
        _usuarioService = usuarioService;
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
    }

    [HttpGet("me")]
    public async Task<ActionResult<AuthMeResponse>> GetMe()
    {
        var firebaseUid = User.FindFirst("firebase_uid")?.Value;
        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized(new { message = "Token sin uid de Firebase" });

        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        var usuario = await _usuarioService.GetByFirebaseUidAsync(firebaseUid);

        if (usuario != null)
        {
            return Ok(new AuthMeResponse
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                RolNombre = usuario.RolNombre,
                EsNuevo = false
            });
        }

        var rolDefault = await _rolRepository.GetByNombreAsync("Empleado");
        if (rolDefault == null)
            return Conflict(new { message = "No existe el rol por defecto. Contacte al administrador." });

        var nuevoUsuario = await _usuarioService.CreateAsync(new UsuarioCreateDto
        {
            FirebaseUid = firebaseUid,
            Email = email,
            Nombre = email.Split('@')[0],
            Apellido = string.Empty,
            NombreUsuario = email.Split('@')[0],
            RolId = rolDefault.Id
        });

        return Ok(new AuthMeResponse
        {
            Id = nuevoUsuario.Id,
            Nombre = nuevoUsuario.Nombre,
            Apellido = nuevoUsuario.Apellido,
            Email = nuevoUsuario.Email,
            RolNombre = nuevoUsuario.RolNombre,
            EsNuevo = true
        });
    }
}

public class AuthMeResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
    public bool EsNuevo { get; set; }
}
