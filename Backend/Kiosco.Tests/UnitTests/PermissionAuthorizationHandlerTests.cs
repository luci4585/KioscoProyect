using System.Security.Claims;
using FluentAssertions;
using Kiosco.API.Authorization;
using Kiosco.Domain.Entities;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de la Etapa 3 (RoadMap §6 — Testing):
/// 4. Roles
/// 5. Permisos
/// Validan la autorización por permisos granulares del usuario interno
/// (RF-006, RF-007, RF-008, RNF-032, RNF-033, RNF-034) sobre base en memoria.
/// </summary>
public class PermissionAuthorizationHandlerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _handler = new PermissionAuthorizationHandler(new UsuarioRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    /// <summary>Crea permisos, roles, usuario y la relación rol-permiso.</summary>
    private Usuario SeedUsuario(
        string firebaseUid,
        string rolNombre,
        string[] permisosCodes,
        bool usuarioActivo = true,
        bool rolActivo = true)
    {
        var permisos = permisosCodes.Select(c => new Permiso
        {
            Codigo = c,
            Nombre = c,
            Activo = true
        }).ToList();

        var rol = new Rol
        {
            Nombre = rolNombre,
            Descripcion = $"Rol {rolNombre}",
            Activo = rolActivo,
            RolPermisos = permisos.Select(p => new RolPermiso { Permiso = p }).ToList()
        };

        var usuario = new Usuario
        {
            Nombre = "Test",
            Apellido = "Usuario",
            NombreUsuario = firebaseUid,
            Email = $"{firebaseUid}@test.local",
            FirebaseUid = firebaseUid,
            Activo = usuarioActivo,
            Rol = rol
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();
        return usuario;
    }

    private ClaimsPrincipal BuildPrincipal(string? firebaseUid)
    {
        var claims = new List<Claim>();
        if (firebaseUid != null)
            claims.Add(new Claim("firebase_uid", firebaseUid));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
    }

    private async Task<bool> AuthorizeAsync(string? firebaseUid, string permisoRequerido)
    {
        var requirement = new PermissionRequirement(permisoRequerido);
        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { requirement },
            BuildPrincipal(firebaseUid),
            resource: null);

        await _handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    // ── Roles (RF-006, RNF-032) ─────────────────────────────────────────────

    [Fact]
    public async Task RolAdministrador_TieneAcceso_ALosPermisosDeSuRol()
    {
        SeedUsuario("uid-admin", "Administrador", new[] { "rubros.gestionar", "ventas.crear" });

        (await AuthorizeAsync("uid-admin", "rubros.gestionar")).Should().BeTrue();
        (await AuthorizeAsync("uid-admin", "ventas.crear")).Should().BeTrue();
    }

    [Fact]
    public async Task RolEmpleado_NoTieneAcceso_APermisosQueNoPosee()
    {
        // Empleado solo tiene "ventas.crear" (semillas: RolPermisoConfiguration).
        SeedUsuario("uid-empleado", "Empleado", new[] { "ventas.crear" });

        (await AuthorizeAsync("uid-empleado", "ventas.crear")).Should().BeTrue();
        (await AuthorizeAsync("uid-empleado", "rubros.gestionar")).Should().BeFalse(
            "el rol Empleado no posee el permiso rubros.gestionar");
        (await AuthorizeAsync("uid-empleado", "usuarios.gestionar")).Should().BeFalse(
            "el rol Empleado no posee el permiso usuarios.gestionar");
    }

    // ── Permisos (RF-008, RNF-033) ──────────────────────────────────────────

    [Fact]
    public async Task PermisoGranular_DiferentesPermisosDelMismoRol_SevaluanPorCodigo()
    {
        SeedUsuario("uid-operador", "Operador",
            new[] { "rubros.gestionar", "productos.crear", "informes.consultar" });

        (await AuthorizeAsync("uid-operador", "productos.crear")).Should().BeTrue();
        (await AuthorizeAsync("uid-operador", "informes.consultar")).Should().BeTrue();
        (await AuthorizeAsync("uid-operador", "productos.eliminar")).Should().BeFalse(
            "el permiso 'productos.eliminar' no está asociado al rol");
    }

    [Fact]
    public async Task UsuarioDesconocido_SinUsuarioInterno_NoObtienePermisos()
    {
        // RNF-029: sin usuario interno en PostgreSQL no hay autorización.
        (await AuthorizeAsync("uid-inexistente", "rubros.gestionar")).Should().BeFalse();
    }

    [Fact]
    public async Task SinClaimDeFirebaseUid_NoObtienePermisos()
    {
        (await AuthorizeAsync(firebaseUid: null, "rubros.gestionar")).Should().BeFalse();
    }

    // ── Baja lógica (RF-005) ────────────────────────────────────────────────

    [Fact]
    public async Task UsuarioDeshabilitado_NoObtieneAcceso_PeseATenerPermisos()
    {
        SeedUsuario("uid-baja", "Administrador",
            new[] { "rubros.gestionar" }, usuarioActivo: false);

        (await AuthorizeAsync("uid-baja", "rubros.gestionar")).Should().BeFalse(
            "un usuario con baja lógica no debe conservar acceso (RF-005)");
    }

    [Fact]
    public async Task RolDeshabilitado_NoOtorgaAcceso()
    {
        SeedUsuario("uid-rol-baja", "RolInactivo",
            new[] { "rubros.gestionar" }, rolActivo: false);

        (await AuthorizeAsync("uid-rol-baja", "rubros.gestionar")).Should().BeFalse(
            "un rol deshabilitado no debe otorgar permisos");
    }
}
