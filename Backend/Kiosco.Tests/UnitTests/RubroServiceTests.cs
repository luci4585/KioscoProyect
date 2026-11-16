using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de servicio de rubros — Etapa 4 (RF-009 registro, RF-010 consulta,
/// RF-011 modificación, RF-012 baja lógica, RF-076 validación).
/// Sobre base de datos en memoria (estrategia de testing del SDD §32.1/§32.2).
/// </summary>
public class RubroServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly RubroService _service;

    public RubroServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new RubroService(new RubroRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    // ── RF-009: registro ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreaElRubro()
    {
        var dto = new RubroCreateDto { Nombre = "Bebidas Energéticas", Descripcion = "Red Bull, Speed" };

        var resultado = await _service.CreateAsync(dto);

        resultado.Id.Should().BeGreaterThan(0);
        resultado.Nombre.Should().Be("Bebidas Energéticas");
        resultado.Activo.Should().BeTrue();

        var enDb = await _context.Rubros.FindAsync(resultado.Id);
        enDb.Should().NotBeNull("el rubro debe persistirse");
        enDb!.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_NombreDuplicado_LanzaBusinessRule()
    {
        await _service.CreateAsync(new RubroCreateDto { Nombre = "Golosinas" });

        var act = () => _service.CreateAsync(new RubroCreateDto { Nombre = "Golosinas" });

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-076/RF-077: no deben existir rubros activos con el mismo nombre");
    }

    // ── RF-010: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DevuelveSoloRubrosActivos()
    {
        await _service.CreateAsync(new RubroCreateDto { Nombre = "Activo" });
        var baja = await _service.CreateAsync(new RubroCreateDto { Nombre = "Inactivo" });
        await _service.DeleteAsync(baja.Id);

        var todos = (await _service.GetAllAsync()).ToList();

        todos.Should().ContainSingle(r => r.Nombre == "Activo");
        todos.Should().NotContain(r => r.Id == baja.Id);
    }

    [Fact]
    public async Task GetByIdAsync_DevuelveElRubroExistente()
    {
        var creado = await _service.CreateAsync(new RubroCreateDto { Nombre = "Limpieza" });

        var resultado = await _service.GetByIdAsync(creado.Id);

        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Limpieza");
    }

    [Fact]
    public async Task GetByIdAsync_RubroInexistente_DevuelveNull()
    {
        (await _service.GetByIdAsync(9999)).Should().BeNull();
    }

    // ── RF-011: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ModificaNombreYDescripcion()
    {
        var creado = await _service.CreateAsync(new RubroCreateDto { Nombre = "Viejo" });

        await _service.UpdateAsync(creado.Id, new RubroUpdateDto
        {
            Nombre = "Nuevo Nombre",
            Descripcion = "Descripción actualizada"
        });

        var enDb = await _context.Rubros.FindAsync(creado.Id);
        enDb!.Nombre.Should().Be("Nuevo Nombre");
        enDb.Descripcion.Should().Be("Descripción actualizada");
    }

    [Fact]
    public async Task UpdateAsync_RubroInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.UpdateAsync(9999, new RubroUpdateDto { Nombre = "X" });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_NombreDuplicadoOTroRubro_LanzaBusinessRule()
    {
        await _service.CreateAsync(new RubroCreateDto { Nombre = "Snacks" });
        var otro = await _service.CreateAsync(new RubroCreateDto { Nombre = "Otros" });

        var act = () => _service.UpdateAsync(otro.Id, new RubroUpdateDto { Nombre = "Snacks" });

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task UpdateAsync_MismoRubro_MantieneSuPropioNombre()
    {
        var creado = await _service.CreateAsync(new RubroCreateDto { Nombre = "Lácteos" });

        var act = async () => await _service.UpdateAsync(creado.Id, new RubroUpdateDto
        {
            Nombre = "Lácteos",
            Descripcion = "Leche y quesos"
        });

        await act.Should().NotThrowAsync("editar un rubro conservando su propio nombre no es duplicado");

        (await _service.GetByIdAsync(creado.Id))!.Descripcion.Should().Be("Leche y quesos");
    }

    // ── RF-012 / RF-073 / RF-074: baja lógica ───────────────────────────────

    [Fact]
    public async Task DeleteAsync_DeshabilitaSinEliminarFisicamente()
    {
        var creado = await _service.CreateAsync(new RubroCreateDto { Nombre = "Temporal" });

        await _service.DeleteAsync(creado.Id);

        var enDb = await _context.Rubros
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == creado.Id);

        enDb.Should().NotBeNull("la baja debe ser lógica, nunca eliminación física (RF-073)");
        enDb!.Activo.Should().BeFalse();
        enDb.Nombre.Should().Be("Temporal", "la información histórica se conserva (RF-074)");
    }

    [Fact]
    public async Task DeleteAsync_RubroInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.DeleteAsync(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RubroDadoDeBaja_PuedeReusarSuNombre_ConOtroRubroNuevo()
    {
        // RF-077: la restricción es sobre rubros activos; el histórico conserva el nombre.
        var original = await _service.CreateAsync(new RubroCreateDto { Nombre = "Promo" });
        await _service.DeleteAsync(original.Id);

        var act = async () => await _service.CreateAsync(new RubroCreateDto { Nombre = "Promo" });

        await act.Should().NotThrowAsync(
            "la validación de unicidad solo considera rubros activos");
    }
}
