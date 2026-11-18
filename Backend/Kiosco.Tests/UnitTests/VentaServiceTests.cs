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
/// Tests de servicio de ventas — Etapa 9 (RoadMap §12; RF-040 a RF-050).
/// Cálculos, stock insuficiente, persistencia, stock, cliente, usuario y anulación.
/// Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class VentaServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly VentaService _service;

    public VentaServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new VentaService(
            new VentaRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context),
            new ClienteRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedUsuarioAsync(string firebaseUid = "uid-vendedor")
    {
        var rol = new Rol { Nombre = $"Rol {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nombre = "Vendedor",
            Apellido = "De Prueba",
            NombreUsuario = $"vend{Guid.NewGuid():N}"[..12],
            Email = $"vend-{Guid.NewGuid():N}@test.local",
            FirebaseUid = firebaseUid,
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    private async Task<int> SeedClienteAsync(string nombre = "Cliente", string apellido = "Ejemplo")
    {
        var cliente = new Cliente { Nombre = nombre, Apellido = apellido, Documento = "30111222" };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente.Id;
    }

    private async Task<int> SeedProductoAsync(
        string codigo = "VEN-1",
        decimal precioVenta = 100,
        int stockCantidad = 10)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Venta",
            Codigo = codigo,
            RubroId = rubro.Id,
            PrecioVenta = precioVenta,
            Costo = 60,
            StockMinimo = 2
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = stockCantidad });
        await _context.SaveChangesAsync();

        return producto.Id;
    }

    // ── RF-042 / RF-043: cálculos (RoadMap §12) ─────────────────────────────

    [Fact]
    public async Task CreateAsync_CalculaSubtotalesYTotal()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoA = await SeedProductoAsync(codigo: "A", precioVenta: 100, stockCantidad: 50);
        var productoB = await SeedProductoAsync(codigo: "B", precioVenta: 50, stockCantidad: 50);

        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoA, Cantidad = 2 }, // 200
                new() { ProductoId = productoB, Cantidad = 3 }  // 150
            }
        }, usuarioId);

        venta.Total.Should().Be(350m, "RF-043: suma de subtotales");
        venta.Detalles.Should().HaveCount(2);
        venta.Detalles.Single(d => d.ProductoId == productoA).Subtotal.Should().Be(200m, "RF-042");
        venta.Detalles.Single(d => d.ProductoId == productoB).Subtotal.Should().Be(150m, "RF-042");
    }

    // ── RF-047 / RoadMap: descuento automático de stock ─────────────────────

    [Fact]
    public async Task CreateAsync_ActualizaElStockAutomaticamente()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10);

        await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 4 }
            }
        }, usuarioId);

        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(6, "RF-047: 10 − 4");
    }

    [Fact]
    public async Task CreateAsync_StockInsuficiente_LanzaBusinessRule_NoPersiste()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 1);

        var act = () => _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 5 }
            }
        }, usuarioId);

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-025/RF-077: no se puede vender más stock del disponible");

        (await _context.Ventas.CountAsync()).Should().Be(0, "la venta no debe persistirse");
        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(1, "el stock no debe modificarse");
    }

    // ── RF-045 / RF-046: cliente y usuario vendedor ─────────────────────────

    [Fact]
    public async Task CreateAsync_RegistraElUsuarioVendedor()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();

        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Débito",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        venta.UsuarioId.Should().Be(usuarioId, "RF-046");
        venta.UsuarioNombre.Should().Be("Vendedor De Prueba", "RF-046: nombre del vendedor");
    }

    [Fact]
    public async Task CreateAsync_AsociaElClienteRegistrado()
    {
        var usuarioId = await SeedUsuarioAsync();
        var clienteId = await SeedClienteAsync("María", "Gómez");
        var productoId = await SeedProductoAsync();

        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            ClienteId = clienteId,
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        venta.ClienteId.Should().Be(clienteId, "RF-045");
        venta.ClienteNombre.Should().Be("María Gómez", "RF-045");
    }

    [Fact]
    public async Task CreateAsync_SinCliente_EsValido()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();

        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            ClienteId = null,
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        venta.ClienteId.Should().BeNull("RF-045: la asociación de cliente es opcional");
    }

    [Fact]
    public async Task CreateAsync_ClienteInexistente_LanzaEntityNotFound()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();

        var act = () => _service.CreateAsync(new VentaCreateDto
        {
            ClienteId = 9999,
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        await act.Should().ThrowAsync<EntityNotFoundException>(
            "RF-045: el cliente debe estar registrado");
        (await _context.Ventas.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_ProductoInexistente_LanzaEntityNotFound()
    {
        var usuarioId = await SeedUsuarioAsync();

        var act = () => _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = 9999, Cantidad = 1 }
            }
        }, usuarioId);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── Persistencia y consulta (RF-048, RF-049) ────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ConDetalles_PersisteLosImportes()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(precioVenta: 120);
        var creada = await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Transferencia",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 3 }
            }
        }, usuarioId);

        var detalle = await _service.GetByIdAsync(creada.Id);

        detalle.Should().NotBeNull();
        detalle!.Total.Should().Be(360m);
        detalle.MetodoPago.Should().Be("Transferencia", "RF-044");
        detalle.Detalles.Should().ContainSingle();
        detalle.Detalles[0].PrecioUnitario.Should().Be(120m);
        detalle.Detalles[0].Subtotal.Should().Be(360m);
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_DevuelveNull()
    {
        (await _service.GetByIdAsync(9999)).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_DevuelveLasVentas()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        var resultados = (await _service.GetAllAsync()).ToList();

        resultados.Should().ContainSingle();
        resultados[0].UsuarioNombre.Should().Be("Vendedor De Prueba");
    }

    // ── RF-050: anulación ───────────────────────────────────────────────────

    [Fact]
    public async Task AnularAsync_AnulaYRestituyeElStock()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10);
        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 4 }
            }
        }, usuarioId);

        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(6);

        await _service.AnularAsync(venta.Id);

        var enDb = await _context.Ventas.IgnoreQueryFilters().FirstAsync(v => v.Id == venta.Id);
        enDb.Anulada.Should().BeTrue("RF-050: conservar la venta con su información");
        var stockRestituido = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stockRestituido.Cantidad.Should().Be(10, "RF-050: restituir las cantidades al stock");
    }

    [Fact]
    public async Task AnularAsync_YaAnulada_LanzaBusinessRule()
    {
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        var venta = await _service.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        await _service.AnularAsync(venta.Id);

        var act = () => _service.AnularAsync(venta.Id);
        await act.Should().ThrowAsync<BusinessRuleException>("no se anula dos veces");
    }

    [Fact]
    public async Task AnularAsync_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _service.AnularAsync(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
