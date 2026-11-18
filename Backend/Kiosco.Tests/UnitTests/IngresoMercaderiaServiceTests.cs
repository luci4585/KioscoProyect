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
/// Tests de servicio de ingreso de mercadería — Etapa 8 (RoadMap §11;
/// RF-036 a RF-039). Verificación central: el ingreso actualiza el stock (RF-038).
/// Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class IngresoMercaderiaServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly IngresoMercaderiaService _service;

    public IngresoMercaderiaServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new IngresoMercaderiaService(
            new IngresoMercaderiaRepository(_context),
            new ProveedorRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedProveedorAsync(string razonSocial = "Distribuidora Sur")
    {
        var proveedor = new Proveedor { RazonSocial = razonSocial, Cuit = "30111223344" };
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor.Id;
    }

    private async Task<int> SeedProductoAsync(
        string codigo = "MER-1",
        int stockCantidad = 10,
        bool conFilaStock = true)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Mercadería",
            Codigo = codigo,
            RubroId = rubro.Id,
            PrecioVenta = 150,
            Costo = 90,
            StockMinimo = 2
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        if (conFilaStock)
        {
            _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = stockCantidad });
            await _context.SaveChangesAsync();
        }

        return producto.Id;
    }

    // ── RF-036 / RF-037 / RF-039: registro, proveedor y costos ──────────────

    [Fact]
    public async Task CreateAsync_CreaElIngresoConDetallesYCostos()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();

        var resultado = await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Observaciones = "Pedido semanal",
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 5, CostoUnitario = 80.50m }
            }
        });

        resultado.Id.Should().BeGreaterThan(0);
        resultado.Observaciones.Should().Be("Pedido semanal");
        resultado.Detalles.Should().ContainSingle();
        resultado.Detalles[0].Cantidad.Should().Be(5);
        resultado.Detalles[0].CostoUnitario.Should().Be(80.50m, "RF-039: costo de adquisición");
    }

    [Fact]
    public async Task CreateAsync_AsociaElProveedorCorrespondiente()
    {
        var proveedorId = await SeedProveedorAsync("Alimentos del Norte");
        var productoId = await SeedProductoAsync();

        var resultado = await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 10 }
            }
        });

        resultado.ProveedorId.Should().Be(proveedorId, "RF-037");
        resultado.ProveedorNombre.Should().Be("Alimentos del Norte", "RF-037");
    }

    // ── RF-038: actualización automática de stock (test central §11) ────────

    [Fact]
    public async Task CreateAsync_IncrementaElStockDeLosProductosIngresados()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoA = await SeedProductoAsync(codigo: "A", stockCantidad: 10);
        var productoB = await SeedProductoAsync(codigo: "B", stockCantidad: 3);

        await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoA, Cantidad = 5, CostoUnitario = 80 },
                new() { ProductoId = productoB, Cantidad = 7, CostoUnitario = 60 }
            }
        });

        var stockA = await _context.Stocks.FirstAsync(s => s.ProductoId == productoA);
        var stockB = await _context.Stocks.FirstAsync(s => s.ProductoId == productoB);

        stockA.Cantidad.Should().Be(15, "RF-038: 10 + 5");
        stockB.Cantidad.Should().Be(10, "RF-038: 3 + 7");
    }

    [Fact]
    public async Task CreateAsync_CreaLaFilaDeStockSiNoExiste()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync(conFilaStock: false);

        await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 4, CostoUnitario = 50 }
            }
        });

        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(4, "RF-038: la fila de stock debe nacer con la cantidad ingresada");
    }

    // ── Validaciones de negocio (RF-077) ────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ProveedorInexistente_LanzaEntityNotFound()
    {
        var productoId = await SeedProductoAsync();

        var act = () => _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = 9999,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 1 }
            }
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
        (await _context.IngresosMercaderia.CountAsync()).Should().Be(0, "no debe crearse nada");
    }

    [Fact]
    public async Task CreateAsync_ProductoInexistente_LanzaEntityNotFound_SinTocarStock()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10);

        var act = () => _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 1 },
                new() { ProductoId = 9999, Cantidad = 5, CostoUnitario = 2 }
            }
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();

        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(10, "el stock no debe modificarse si el ingreso falla");
        (await _context.IngresosMercaderia.CountAsync()).Should().Be(0);
    }

    // ── Consulta ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DevuelveLosIngresos()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();
        await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 10 }
            }
        });

        var resultados = (await _service.GetAllAsync()).ToList();

        resultados.Should().ContainSingle();
        resultados[0].ProveedorNombre.Should().Be("Distribuidora Sur");
    }

    [Fact]
    public async Task GetByIdAsync_ConDetallesDevuelveLosProductos()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();
        var creado = await _service.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 6, CostoUnitario = 75 }
            }
        });

        var resultado = await _service.GetByIdAsync(creado.Id);

        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().ContainSingle();
        resultado.Detalles[0].ProductoNombre.Should().Be("Producto Mercadería");
        resultado.Detalles[0].CostoUnitario.Should().Be(75);
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_DevuelveNull()
    {
        (await _service.GetByIdAsync(9999)).Should().BeNull();
    }
}
