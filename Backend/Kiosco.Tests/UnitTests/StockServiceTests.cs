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
/// Tests de servicio de stock — Etapa 7 (RoadMap §10; RF-021, RF-022, RF-023,
/// RF-026, RF-077). Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class StockServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly StockService _service;

    public StockServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new StockService(
            new StockRepository(_context),
            new ProductoRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<Stock> SeedStockAsync(
        string codigo = "STK-1",
        string nombre = "Producto Stock",
        int cantidad = 10,
        int stockMinimo = 3)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = nombre,
            Codigo = codigo,
            RubroId = rubro.Id,
            PrecioVenta = 100,
            Costo = 60,
            StockMinimo = stockMinimo
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var stock = new Stock { ProductoId = producto.Id, Cantidad = cantidad };
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return stock;
    }

    // ── RF-021: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DevuelveStocksConDatosDelProducto()
    {
        await SeedStockAsync(cantidad: 7);

        var resultados = (await _service.GetAllAsync()).ToList();

        resultados.Should().ContainSingle();
        resultados[0].ProductoNombre.Should().Be("Producto Stock");
        resultados[0].Cantidad.Should().Be(7);
    }

    [Fact]
    public async Task GetByProductoAsync_DevuelveElStock()
    {
        var stock = await SeedStockAsync(cantidad: 5);

        var resultado = await _service.GetByProductoAsync(stock.ProductoId);

        resultado.Should().NotBeNull();
        resultado!.Cantidad.Should().Be(5);
    }

    [Fact]
    public async Task GetByProductoAsync_SinStock_DevuelveNull()
    {
        (await _service.GetByProductoAsync(9999)).Should().BeNull();
    }

    // ── RF-022: ingreso (incrementos) ───────────────────────────────────────

    [Fact]
    public async Task IngresarAsync_IncrementaLaCantidad()
    {
        var stock = await SeedStockAsync(cantidad: 10);

        var resultado = await _service.IngresarAsync(stock.ProductoId, 5);

        resultado.Cantidad.Should().Be(15, "RF-022: el ingreso incrementa la cantidad disponible");

        var enDb = await _context.Stocks.FindAsync(stock.Id);
        enDb!.Cantidad.Should().Be(15);
    }

    [Fact]
    public async Task IngresarAsync_ProductoInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.IngresarAsync(9999, 5);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-023: egreso (disminuciones + stock insuficiente) ─────────────────

    [Fact]
    public async Task EgresarAsync_DisminuyeLaCantidad()
    {
        var stock = await SeedStockAsync(cantidad: 10);

        var resultado = await _service.EgresarAsync(stock.ProductoId, 4);

        resultado.Cantidad.Should().Be(6, "RF-023: el egreso disminuye la cantidad disponible");

        var enDb = await _context.Stocks.FindAsync(stock.Id);
        enDb!.Cantidad.Should().Be(6);
    }

    [Fact]
    public async Task EgresarAsync_CantidadExacta_LlegaACero()
    {
        var stock = await SeedStockAsync(cantidad: 4);

        var resultado = await _service.EgresarAsync(stock.ProductoId, 4);

        resultado.Cantidad.Should().Be(0);
    }

    [Fact]
    public async Task EgresarAsync_StockInsuficiente_LanzaBusinessRule()
    {
        var stock = await SeedStockAsync(cantidad: 3);

        var act = () => _service.EgresarAsync(stock.ProductoId, 4);

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-077: no se puede egresar más de lo disponible");

        var enDb = await _context.Stocks.FindAsync(stock.Id);
        enDb!.Cantidad.Should().Be(3, "el stock no debe modificarse si el egreso falla");
    }

    [Fact]
    public async Task EgresarAsync_ProductoInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.EgresarAsync(9999, 1);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-026: identificación de stock bajo ────────────────────────────────

    [Fact]
    public async Task GetAlertasAsync_IncluyeCantidadIgualAlMinimo()
    {
        // igual al mínimo → alerta (RF-026: "igual o inferior").
        await SeedStockAsync(codigo: "IGUAL", cantidad: 3, stockMinimo: 3);
        // por debajo del mínimo → alerta.
        await SeedStockAsync(codigo: "BAJO", cantidad: 1, stockMinimo: 3);
        // por encima del mínimo → sin alerta.
        await SeedStockAsync(codigo: "SANO", cantidad: 10, stockMinimo: 3);

        var alertas = (await _service.GetAlertasAsync()).ToList();

        alertas.Should().HaveCount(2, "solo los que están en o bajo el mínimo");
        alertas.Should().Contain(s => s.ProductoCodigo == "IGUAL");
        alertas.Should().Contain(s => s.ProductoCodigo == "BAJO");
        alertas.Should().NotContain(s => s.ProductoCodigo == "SANO");
        alertas.Should().OnlyContain(s => s.StockBajo);
    }
}
