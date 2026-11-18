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
/// Tests de servicio de clientes — Etapa 6 (RoadMap §9; RF-027 a RF-031,
/// RF-073/RF-074). Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class ClienteServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly ClienteService _service;

    public ClienteServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new ClienteService(new ClienteRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<Cliente> SeedClienteAsync(
        string nombre = "Juan",
        string apellido = "Pérez",
        string? documento = "30111222",
        bool activo = true)
    {
        var cliente = new Cliente
        {
            Nombre = nombre,
            Apellido = apellido,
            Documento = documento,
            Telefono = "1144445555",
            Email = "juan@example.com",
            Direccion = "Calle Falsa 123",
            Activo = activo
        };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    // ── RF-027: registro ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreaElCliente()
    {
        var resultado = await _service.CreateAsync(new ClienteCreateDto
        {
            Nombre = "María",
            Apellido = "Gómez",
            Documento = "27888777",
            Email = "maria@example.com"
        });

        resultado.Id.Should().BeGreaterThan(0);
        resultado.Nombre.Should().Be("María");
        resultado.Activo.Should().BeTrue();

        var enDb = await _context.Clientes.FindAsync(resultado.Id);
        enDb.Should().NotBeNull();
        enDb!.Documento.Should().Be("27888777");
    }

    // ── RF-028: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DevuelveSoloClientesActivos()
    {
        await SeedClienteAsync(nombre: "Activo", documento: "111");
        await SeedClienteAsync(nombre: "DadoDeBaja", documento: "222", activo: false);

        var resultados = (await _service.GetAllAsync()).ToList();

        resultados.Should().ContainSingle(c => c.Nombre == "Activo");
        resultados.Should().NotContain(c => c.Nombre == "DadoDeBaja");
    }

    [Fact]
    public async Task GetByIdAsync_DevuelveElCliente()
    {
        var cliente = await SeedClienteAsync();

        var resultado = await _service.GetByIdAsync(cliente.Id);

        resultado.Should().NotBeNull();
        resultado!.Apellido.Should().Be("Pérez");
    }

    [Fact]
    public async Task GetByIdAsync_ClienteInexistente_DevuelveNull()
    {
        (await _service.GetByIdAsync(9999)).Should().BeNull();
    }

    // ── RF-029: búsqueda ────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_FiltraPorNombreOApellido()
    {
        await SeedClienteAsync(nombre: "Carlos", apellido: "López", documento: "333");
        await SeedClienteAsync(nombre: "Ana", apellido: "Torres", documento: "444");

        var porNombre = (await _service.SearchAsync("Carlos", null)).ToList();
        porNombre.Should().ContainSingle(c => c.Documento == "333");

        var porApellido = (await _service.SearchAsync("Torres", null)).ToList();
        porApellido.Should().ContainSingle(c => c.Documento == "444");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorDocumento()
    {
        await SeedClienteAsync(nombre: "Uno", documento: "30555666");
        await SeedClienteAsync(nombre: "Dos", documento: "30777888");

        var resultados = (await _service.SearchAsync(null, "555")).ToList();

        resultados.Should().ContainSingle(c => c.Nombre == "Uno");
    }

    [Fact]
    public async Task SearchAsync_SoloDevuelveActivos()
    {
        await SeedClienteAsync(nombre: "Baja", documento: "999", activo: false);

        (await _service.SearchAsync(null, "999")).Should().BeEmpty(
            "la búsqueda no debe devolver clientes dados de baja");
    }

    // ── RF-030: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ModificaLosDatos()
    {
        var cliente = await SeedClienteAsync();

        await _service.UpdateAsync(cliente.Id, new ClienteUpdateDto
        {
            Nombre = "Juan Modificado",
            Apellido = "Pérez",
            Documento = "30111222",
            Email = "nuevo@example.com"
        });

        var enDb = await _context.Clientes.FindAsync(cliente.Id);
        enDb!.Nombre.Should().Be("Juan Modificado");
        enDb.Email.Should().Be("nuevo@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ClienteInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.UpdateAsync(9999, new ClienteUpdateDto
        {
            Nombre = "X",
            Apellido = "Y"
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-031 / RF-073 / RF-074: baja lógica ──────────────────────────────

    [Fact]
    public async Task DeleteAsync_DeshabilitaSinEliminarFisicamente()
    {
        var cliente = await SeedClienteAsync();

        await _service.DeleteAsync(cliente.Id);

        var enDb = await _context.Clientes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cliente.Id);
        enDb.Should().NotBeNull("RF-073: baja lógica, nunca eliminación física");
        enDb!.Activo.Should().BeFalse();
        enDb.Nombre.Should().Be("Juan", "RF-074: conservar información histórica");
    }

    [Fact]
    public async Task DeleteAsync_ClienteInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.DeleteAsync(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
