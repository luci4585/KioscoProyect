using FluentAssertions;
using Kiosco.API.Controllers;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Application.Validators;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Tests de controller de stock con base de datos en memoria —
/// Etapa 7 (RF-021, RF-022, RF-023, RF-026, RNF-017 códigos HTTP).
/// </summary>
public class StockControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly StockController _controller;

    public StockControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new StockController(
            new StockService(new StockRepository(_context), new ProductoRepository(_context)),
            new StockMovimientoValidator());
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedProductoConStockAsync(int cantidad = 10, int stockMinimo = 3)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Stock",
            Codigo = $"STK-{Guid.NewGuid():N}"[..10],
            RubroId = rubro.Id,
            PrecioVenta = 100,
            Costo = 60,
            StockMinimo = stockMinimo
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = cantidad });
        await _context.SaveChangesAsync();

        return producto.Id;
    }

    // ── RF-021: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetStock_Return200ConLista()
    {
        await SeedProductoConStockAsync();

        var result = await _controller.GetStock();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<StockDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task GetStockByProducto_Existente_Return200()
    {
        var productoId = await SeedProductoConStockAsync(cantidad: 8);

        var result = await _controller.GetStockByProducto(productoId);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((StockDto)ok.Value!).Cantidad.Should().Be(8);
    }

    [Fact]
    public async Task GetStockByProducto_Inexistente_Return404()
    {
        var result = await _controller.GetStockByProducto(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── RF-026: alertas ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAlertas_Return200SoloConStockBajo()
    {
        await SeedProductoConStockAsync(cantidad: 1, stockMinimo: 3); // bajo
        await SeedProductoConStockAsync(cantidad: 50, stockMinimo: 3); // sano

        var result = await _controller.GetAlertas();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var lista = ((IEnumerable<StockDto>)ok.Value!).ToList();
        lista.Should().ContainSingle();
        lista[0].StockBajo.Should().BeTrue();
    }

    // ── RF-022: ingreso ─────────────────────────────────────────────────────

    [Fact]
    public async Task Ingresar_DatosValidos_Return200ConStockActualizado()
    {
        var productoId = await SeedProductoConStockAsync(cantidad: 10);

        var result = await _controller.Ingresar(new StockMovimientoDto
        {
            ProductoId = productoId,
            Cantidad = 5
        });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((StockDto)ok.Value!).Cantidad.Should().Be(15);
        (await _context.Stocks.FirstAsync(s => s.ProductoId == productoId))
            .Cantidad.Should().Be(15);
    }

    [Fact]
    public async Task Ingresar_CantidadInvalida_Return400ValidationProblem()
    {
        var result = await _controller.Ingresar(new StockMovimientoDto
        {
            ProductoId = 1,
            Cantidad = 0
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Ingresar_ProductoInexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.Ingresar(new StockMovimientoDto
        {
            ProductoId = 9999,
            Cantidad = 1
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-023: egreso ──────────────────────────────────────────────────────

    [Fact]
    public async Task Egresar_DatosValidos_Return200ConStockActualizado()
    {
        var productoId = await SeedProductoConStockAsync(cantidad: 10);

        var result = await _controller.Egresar(new StockMovimientoDto
        {
            ProductoId = productoId,
            Cantidad = 4
        });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((StockDto)ok.Value!).Cantidad.Should().Be(6);
    }

    [Fact]
    public async Task Egresar_StockInsuficiente_LanzaBusinessRule()
    {
        var productoId = await SeedProductoConStockAsync(cantidad: 3);

        var act = () => _controller.Egresar(new StockMovimientoDto
        {
            ProductoId = productoId,
            Cantidad = 10
        });

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-077: el egreso supera el stock disponible");
    }

    [Fact]
    public async Task Egresar_CantidadInvalida_Return400ValidationProblem()
    {
        var result = await _controller.Egresar(new StockMovimientoDto
        {
            ProductoId = 1,
            Cantidad = -1
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }
}
