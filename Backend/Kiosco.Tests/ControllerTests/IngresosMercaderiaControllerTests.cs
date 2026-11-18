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
/// Tests de controller de ingresos de mercadería con base de datos en memoria —
/// Etapa 8 (RF-036 a RF-039, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class IngresosMercaderiaControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly IngresosMercaderiaController _controller;

    public IngresosMercaderiaControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new IngresosMercaderiaController(
            new IngresoMercaderiaService(
                new IngresoMercaderiaRepository(_context),
                new ProveedorRepository(_context),
                new ProductoRepository(_context),
                new StockRepository(_context)),
            new IngresoMercaderiaCreateValidator());
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedProveedorAsync()
    {
        var proveedor = new Proveedor { RazonSocial = "Distribuidora Sur", Cuit = "30111223344" };
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor.Id;
    }

    private async Task<int> SeedProductoAsync(int stockCantidad = 10)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Mercadería",
            Codigo = $"MER-{Guid.NewGuid():N}"[..9],
            RubroId = rubro.Id,
            PrecioVenta = 150,
            Costo = 90,
            StockMinimo = 2
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = stockCantidad });
        await _context.SaveChangesAsync();

        return producto.Id;
    }

    // ── RF-036 / RF-038: alta con actualización de stock ────────────────────

    [Fact]
    public async Task Create_DatosValidos_Return201Created_YActualizaStock()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10);

        var result = await _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Observaciones = "Pedido",
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 5, CostoUnitario = 80 }
            }
        });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        ((IngresoMercaderiaDto)created.Value!).ProveedorNombre.Should().Be("Distribuidora Sur");

        // RF-038: el ingreso debe haber incrementado el stock.
        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(15, "10 + 5 (RF-038)");
    }

    [Fact]
    public async Task Create_SinDetalles_Return400ValidationProblem()
    {
        var proveedorId = await SeedProveedorAsync();

        var result = await _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>()
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_CantidadInvalida_Return400ValidationProblem()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();

        var result = await _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 0, CostoUnitario = 10 }
            }
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Create_ProveedorInexistente_LanzaEntityNotFound()
    {
        var productoId = await SeedProductoAsync();

        var act = () => _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = 9999,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 1 }
            }
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── Consulta ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetIngresos_Return200ConLista()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();
        await _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1, CostoUnitario = 10 }
            }
        });

        var result = await _controller.GetIngresos();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<IngresoMercaderiaDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task GetIngreso_Existente_Return200ConDetalles()
    {
        var proveedorId = await SeedProveedorAsync();
        var productoId = await SeedProductoAsync();
        var creado = await _controller.CreateIngreso(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedorId,
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 3, CostoUnitario = 70 }
            }
        });
        var id = ((IngresoMercaderiaDto)((CreatedAtActionResult)creado.Result!).Value!).Id;

        var result = await _controller.GetIngreso(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (IngresoMercaderiaDto)ok.Value!;
        dto.Detalles.Should().ContainSingle();
        dto.Detalles[0].CostoUnitario.Should().Be(70, "RF-039");
    }

    [Fact]
    public async Task GetIngreso_Inexistente_Return404()
    {
        var result = await _controller.GetIngreso(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
