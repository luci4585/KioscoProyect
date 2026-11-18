using FluentAssertions;
using Kiosco.API.Controllers;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Application.Validators;
using Kiosco.Domain.Entities;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Kiosco.Tests.UnitTests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Tests de controller de productos con base de datos en memoria —
/// Etapa 5 (RF-013 a RF-019, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class ProductosControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly ProductosController _controller;

    public ProductosControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new ProductosController(
            new ProductoService(
                new ProductoRepository(_context),
                new RubroRepository(_context),
                new StockRepository(_context)),
            new ProductoCreateValidator(),
            new ProductoUpdateValidator());
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedRubroAsync()
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();
        return rubro.Id;
    }

    // ── RF-013 / RNF-017: alta y códigos HTTP ──────────────────────────────

    [Fact]
    public async Task Create_DatosValidos_Return201Created()
    {
        var rubroId = await SeedRubroAsync();

        var result = await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Alfajor",
            Codigo = "AFJ-1",
            RubroId = rubroId,
            PrecioVenta = 100,
            Costo = 60,
            StockMinimo = 2
        });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        ((ProductoDto)created.Value!).Codigo.Should().Be("AFJ-1");
    }

    [Fact]
    public async Task Create_DatosInvalidos_Return400ValidationProblem()
    {
        var result = await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "",
            Codigo = "",
            RubroId = 0,
            PrecioVenta = -1,
            Costo = -1
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    // ── RF-015 / RF-016: consulta y búsqueda ───────────────────────────────

    [Fact]
    public async Task GetProductos_Return200ConLista()
    {
        var rubroId = await SeedRubroAsync();
        await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Papas",
            Codigo = "PAP-1",
            RubroId = rubroId,
            PrecioVenta = 50,
            Costo = 30
        });

        var result = await _controller.GetProductos();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ProductoDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task Search_ConFiltros_DevuelveSoloCoincidencias()
    {
        var rubroId = await SeedRubroAsync();
        await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Coca Cola",
            Codigo = "BEB-01",
            RubroId = rubroId,
            PrecioVenta = 100,
            Costo = 60
        });
        await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Papas",
            Codigo = "SNK-01",
            RubroId = rubroId,
            PrecioVenta = 50,
            Costo = 30
        });

        var result = await _controller.Search(nombre: "Coca", codigo: null,
            rubroId: null, activo: null, conStock: null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ProductoDto>)ok.Value!).Should().ContainSingle(p => p.Codigo == "BEB-01");
    }

    [Fact]
    public async Task GetById_Inexistente_Return404NotFound()
    {
        var result = await _controller.GetProducto(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── RF-017 / RF-018: modificación y baja ───────────────────────────────

    [Fact]
    public async Task Update_Existente_Return204NoContent()
    {
        var rubroId = await SeedRubroAsync();
        var created = await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Viejo",
            Codigo = "V-1",
            RubroId = rubroId,
            PrecioVenta = 10,
            Costo = 5
        });
        var id = ((ProductoDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.UpdateProducto(id, new ProductoUpdateDto
        {
            Nombre = "Nuevo",
            Codigo = "V-1",
            RubroId = rubroId,
            PrecioVenta = 20,
            Costo = 8
        });

        result.Should().BeOfType<NoContentResult>();
        (await _context.Productos.FindAsync(id))!.Nombre.Should().Be("Nuevo");
    }

    [Fact]
    public async Task Delete_Existente_Return204NoContent()
    {
        var rubroId = await SeedRubroAsync();
        var created = await _controller.CreateProducto(new ProductoCreateDto
        {
            Nombre = "Temporal",
            Codigo = "T-1",
            RubroId = rubroId,
            PrecioVenta = 1,
            Costo = 1
        });
        var id = ((ProductoDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.DeleteProducto(id);

        result.Should().BeOfType<NoContentResult>();
        (await _context.Productos.IgnoreQueryFilters().FirstAsync(p => p.Id == id))
            .Activo.Should().BeFalse("RF-018: baja lógica");
    }
}
