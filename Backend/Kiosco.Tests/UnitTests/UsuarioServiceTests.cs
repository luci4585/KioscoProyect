using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de servicio de usuario — Etapa 3 (RF-004 modificación, RF-005 baja
/// lógica, RF-006 asignación de roles, RF-076 validación de datos).
/// Sobre base de datos en memoria (estrategia de testing del SDD §32.2).
/// </summary>
public class UsuarioServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new UsuarioService(
            new UsuarioRepository(_context),
            new RolRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private Usuario SeedUsuario(string email = "user@test.local", string nombreUsuario = "user1")
    {
        var rol = new Rol { Nombre = "Rol Test", Descripcion = "Rol para tests" };
        var usuario = new Usuario
        {
            Nombre = "Test",
            Apellido = "Usuario",
            NombreUsuario = nombreUsuario,
            Email = email,
            FirebaseUid = $"uid-{Guid.NewGuid():N}",
            Rol = rol
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();
        return usuario;
    }

    // ── RF-004: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ModificaLosDatosDelUsuario()
    {
        var usuario = SeedUsuario();

        var resultado = await _service.UpdateAsync(usuario.Id, new UsuarioUpdateDto
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            NombreUsuario = "jperez",
            Email = "jperez@test.local"
        });

        resultado.Nombre.Should().Be("Juan");
        resultado.Apellido.Should().Be("Pérez");
        resultado.NombreUsuario.Should().Be("jperez");
        resultado.Email.Should().Be("jperez@test.local");

        var enDb = await _context.Usuarios.FindAsync(usuario.Id);
        enDb!.Nombre.Should().Be("Juan", "los cambios deben persistirse");
    }

    [Fact]
    public async Task UpdateAsync_UsuarioInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.UpdateAsync(9999, new UsuarioUpdateDto
        {
            Nombre = "X",
            Apellido = "Y",
            NombreUsuario = "xy",
            Email = "xy@test.local"
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_EmailDuplicado_LanzaBusinessRule()
    {
        SeedUsuario(email: "ocupado@test.local", nombreUsuario: "orig");
        var usuario = SeedUsuario(email: "libre@test.local", nombreUsuario: "target");

        var act = () => _service.UpdateAsync(usuario.Id, new UsuarioUpdateDto
        {
            Nombre = "T",
            Apellido = "T",
            NombreUsuario = "target",
            Email = "ocupado@test.local" // ya usado por otro usuario
        });

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    // ── RF-005: baja lógica ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_DeshabilitaSinEliminarFisicamente()
    {
        var usuario = SeedUsuario();

        await _service.DeleteAsync(usuario.Id);

        var enDb = await _context.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuario.Id);

        enDb.Should().NotBeNull("la baja debe ser lógica, nunca eliminación física (RF-005, RF-073)");
        enDb!.Activo.Should().BeFalse();
        enDb.Email.Should().Be(usuario.Email, "la información histórica se conserva (RF-074)");
    }

    [Fact]
    public async Task DeleteAsync_UsuarioInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.DeleteAsync(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-006: asignación de roles ────────────────────────────────────────

    [Fact]
    public async Task UpdateRolAsync_CambiaElRolDelUsuario()
    {
        var usuario = SeedUsuario();
        var otroRol = new Rol { Nombre = "Rol Nuevo", Descripcion = "Segundo rol" };
        _context.Roles.Add(otroRol);
        await _context.SaveChangesAsync();

        var resultado = await _service.UpdateRolAsync(
            usuario.Id, new UsuarioRolUpdateDto { RolId = otroRol.Id });

        resultado.RolId.Should().Be(otroRol.Id);
        resultado.RolNombre.Should().Be("Rol Nuevo");
    }

    [Fact]
    public async Task UpdateRolAsync_RolInexistente_LanzaEntityNotFound()
    {
        var usuario = SeedUsuario();

        var act = () => _service.UpdateRolAsync(
            usuario.Id, new UsuarioRolUpdateDto { RolId = 9999 });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-003/0079: existencia y unicidad ─────────────────────────────────

    [Fact]
    public async Task GetAllAsync_NoIncluyeUsuariosDeshabilitados()
    {
        var activo = SeedUsuario(email: "activo@test.local", nombreUsuario: "a1");
        var baja = SeedUsuario(email: "baja@test.local", nombreUsuario: "b1");
        await _service.DeleteAsync(baja.Id);

        var todos = (await _service.GetAllAsync()).ToList();

        todos.Should().ContainSingle(u => u.Id == activo.Id);
        todos.Should().NotContain(u => u.Id == baja.Id);
    }
}
