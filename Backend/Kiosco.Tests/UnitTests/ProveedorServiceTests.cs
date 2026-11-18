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
/// Tests de servicio de proveedores — Etapa 6 (RoadMap §9; RF-032 a RF-035,
/// RF-073/RF-074, unicidad de CUIT). Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class ProveedorServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly ProveedorService _service;

    public ProveedorServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new ProveedorService(new ProveedorRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<Proveedor> SeedProveedorAsync(
        string razonSocial = "Distribuidora Sur",
        string? cuit = "30711223344",
        bool activo = true)
    {
        var proveedor = new Proveedor
        {
            RazonSocial = razonSocial,
            Cuit = cuit,
            Telefono = "1133334444",
            Email = "contacto@distribuidora.com",
            Direccion = "Av. Principal 456",
            Activo = activo
        };
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor;
    }

    // ── RF-032: registro + unicidad de CUIT ─────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreaElProveedor()
    {
        var resultado = await _service.CreateAsync(new ProveedorCreateDto
        {
            RazonSocial = "Alimentos del Norte",
            Cuit = "30999888777",
            Email = "norte@example.com"
        });

        resultado.Id.Should().BeGreaterThan(0);
        resultado.RazonSocial.Should().Be("Alimentos del Norte");
        resultado.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_CuitDuplicado_LanzaBusinessRule()
    {
        await SeedProveedorAsync(cuit: "30123456789");

        var act = () => _service.CreateAsync(new ProveedorCreateDto
        {
            RazonSocial = "Otra Distribuidora",
            Cuit = "30123456789"
        });

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-077: no deben existir dos proveedores con el mismo CUIT");
    }

    // ── RF-033: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DevuelveSoloProveedoresActivos()
    {
        await SeedProveedorAsync(razonSocial: "Activo SA", cuit: "30111000111");
        await SeedProveedorAsync(razonSocial: "DadoDeBaja SA", cuit: "30222000222", activo: false);

        var resultados = (await _service.GetAllAsync()).ToList();

        resultados.Should().ContainSingle(p => p.RazonSocial == "Activo SA");
        resultados.Should().NotContain(p => p.RazonSocial == "DadoDeBaja SA");
    }

    [Fact]
    public async Task GetByIdAsync_DevuelveElProveedor()
    {
        var proveedor = await SeedProveedorAsync();

        var resultado = await _service.GetByIdAsync(proveedor.Id);

        resultado.Should().NotBeNull();
        resultado!.RazonSocial.Should().Be("Distribuidora Sur");
    }

    // ── Búsqueda (RoadMap §9) ───────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_FiltraPorRazonSocial()
    {
        await SeedProveedorAsync(razonSocial: "Bebidas Quilmes", cuit: "30333000333");
        await SeedProveedorAsync(razonSocial: "Snacks Andes", cuit: "30444000444");

        var resultados = (await _service.SearchAsync("Bebidas", null)).ToList();

        resultados.Should().ContainSingle(p => p.RazonSocial == "Bebidas Quilmes");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorCuit()
    {
        await SeedProveedorAsync(razonSocial: "Uno", cuit: "30555000555");
        await SeedProveedorAsync(razonSocial: "Dos", cuit: "30666000666");

        var resultados = (await _service.SearchAsync(null, "555")).ToList();

        resultados.Should().ContainSingle(p => p.RazonSocial == "Uno");
    }

    [Fact]
    public async Task SearchAsync_SoloDevuelveActivos()
    {
        await SeedProveedorAsync(razonSocial: "Baja", cuit: "30777000777", activo: false);

        (await _service.SearchAsync("Baja", null)).Should().BeEmpty(
            "la búsqueda no debe devolver proveedores dados de baja");
    }

    // ── RF-034: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ModificaLosDatos()
    {
        var proveedor = await SeedProveedorAsync();

        await _service.UpdateAsync(proveedor.Id, new ProveedorUpdateDto
        {
            RazonSocial = "Distribuidora Sur Renombrada",
            Cuit = "30711223344",
            Email = "nuevo@example.com"
        });

        var enDb = await _context.Proveedores.FindAsync(proveedor.Id);
        enDb!.RazonSocial.Should().Be("Distribuidora Sur Renombrada");
        enDb.Email.Should().Be("nuevo@example.com");
    }

    [Fact]
    public async Task UpdateAsync_CuitDuplicadoDeOtroProveedor_LanzaBusinessRule()
    {
        await SeedProveedorAsync(razonSocial: "Primero", cuit: "30111222333");
        var segundo = await SeedProveedorAsync(razonSocial: "Segundo", cuit: "30444555666");

        var act = () => _service.UpdateAsync(segundo.Id, new ProveedorUpdateDto
        {
            RazonSocial = "Segundo",
            Cuit = "30111222333"
        });

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task UpdateAsync_MismoCuitDelPropioProveedor_NoFalla()
    {
        var proveedor = await SeedProveedorAsync(cuit: "30999111222");

        var act = () => _service.UpdateAsync(proveedor.Id, new ProveedorUpdateDto
        {
            RazonSocial = "Distribuidora Sur",
            Cuit = "30999111222"
        });

        await act.Should().NotThrowAsync("conservar su propio CUIT no es duplicado");
    }

    [Fact]
    public async Task UpdateAsync_ProveedorInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.UpdateAsync(9999, new ProveedorUpdateDto
        {
            RazonSocial = "X"
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-035 / RF-073 / RF-074: baja lógica ──────────────────────────────

    [Fact]
    public async Task DeleteAsync_DeshabilitaSinEliminarFisicamente()
    {
        var proveedor = await SeedProveedorAsync();

        await _service.DeleteAsync(proveedor.Id);

        var enDb = await _context.Proveedores.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == proveedor.Id);
        enDb.Should().NotBeNull("RF-073: baja lógica, nunca eliminación física");
        enDb!.Activo.Should().BeFalse();
        enDb.RazonSocial.Should().Be("Distribuidora Sur", "RF-074: conservar información histórica");
    }

    [Fact]
    public async Task DeleteAsync_ProveedorInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.DeleteAsync(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
