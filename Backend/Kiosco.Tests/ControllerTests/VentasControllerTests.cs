using System.Security.Claims;
using FluentAssertions;
using Kiosco.API.Controllers;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Application.Validators;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Tests de controller de ventas con base de datos en memoria —
/// Etapa 9 (RF-040 a RF-050, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class VentasControllerTests : IDisposable
{
    private const string FirebaseUid = "uid-venta-test";

    private readonly KioscoDbContext _context;
    private readonly VentasController _controller;
    private readonly VentaService _ventaService;

    public VentasControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _ventaService = new VentaService(
            new VentaRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context),
            new ClienteRepository(_context));

        _controller = new VentasController(
            _ventaService,
            new VentaCreateValidator(),
            new UsuarioService(new UsuarioRepository(_context), new RolRepository(_context)))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    // RF-046: el vendedor sale del claim del usuario autenticado.
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim("firebase_uid", FirebaseUid) }))
                }
            }
        };
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedUsuarioAsync()
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
            FirebaseUid = FirebaseUid,
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    private async Task<int> SeedClienteAsync()
    {
        var cliente = new Cliente { Nombre = "María", Apellido = "Gómez", Documento = "30111222" };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente.Id;
    }

    private async Task<int> SeedProductoAsync(int stockCantidad = 10, decimal precioVenta = 100)
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Venta",
            Codigo = $"VEN-{Guid.NewGuid():N}"[..9],
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

    private static VentaCreateDto NuevaVenta(int productoId, string metodo = "Efectivo") => new()
    {
        MetodoPago = metodo,
        Detalles = new List<DetalleVentaCreateDto>
        {
            new() { ProductoId = productoId, Cantidad = 2 }
        }
    };

    // ── RF-040 a RF-047: registro ───────────────────────────────────────────

    [Fact]
    public async Task Create_DatosValidos_Return201_TotalStockYVendedor()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10, precioVenta: 150);

        var result = await _controller.CreateVenta(NuevaVenta(productoId));

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        var dto = (VentaDto)created.Value!;
        dto.Total.Should().Be(300m, "RF-043: 150 × 2");
        dto.UsuarioNombre.Should().Be("Vendedor De Prueba", "RF-046");

        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(8, "RF-047: 10 − 2");
    }

    [Fact]
    public async Task Create_ConCliente_OpcionalLoAsocia_RF045()
    {
        await SeedUsuarioAsync();
        var clienteId = await SeedClienteAsync();
        var productoId = await SeedProductoAsync();

        var dto = NuevaVenta(productoId);
        dto.ClienteId = clienteId;

        var result = await _controller.CreateVenta(dto);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        ((VentaDto)created.Value!).ClienteNombre.Should().Be("María Gómez");
    }

    [Fact]
    public async Task Create_DatosInvalidos_Return400ValidationProblem()
    {
        await SeedUsuarioAsync();

        var result = await _controller.CreateVenta(new VentaCreateDto
        {
            MetodoPago = "Bitcoin",
            Detalles = new List<DetalleVentaCreateDto>()
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_StockInsuficiente_LanzaBusinessRule()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 1);

        var act = () => _controller.CreateVenta(NuevaVenta(productoId));

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RoadMap §12: validar stock insuficiente");
    }

    [Fact]
    public async Task Create_SinUsuarioInterno_ReturnUnauthorized()
    {
        // Sin claim de firebase_uid → no hay vendedor que registrar (RF-046).
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var productoId = await SeedProductoAsync();

        var result = await _controller.CreateVenta(NuevaVenta(productoId));

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    // ── RF-048 / RF-049: consulta ───────────────────────────────────────────

    [Fact]
    public async Task GetVentas_Return200ConLista()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        await _controller.CreateVenta(NuevaVenta(productoId));

        var result = await _controller.GetVentas();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<VentaDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task GetVenta_Existente_Return200ConDetalles()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(precioVenta: 120);
        var creada = await _controller.CreateVenta(NuevaVenta(productoId));
        var id = ((VentaDto)((CreatedAtActionResult)creada.Result!).Value!).Id;

        var result = await _controller.GetVenta(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (VentaDto)ok.Value!;
        dto.Detalles.Should().ContainSingle();
        dto.Detalles[0].PrecioUnitario.Should().Be(120m, "RF-049: precios visibles");
        dto.Detalles[0].Subtotal.Should().Be(240m, "RF-042");
    }

    [Fact]
    public async Task GetVenta_Inexistente_Return404()
    {
        var result = await _controller.GetVenta(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── RF-050: anulación ───────────────────────────────────────────────────

    [Fact]
    public async Task Anular_VentaActiva_Return204YRestituyeStock()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(stockCantidad: 10);
        var creada = await _controller.CreateVenta(NuevaVenta(productoId));
        var id = ((VentaDto)((CreatedAtActionResult)creada.Result!).Value!).Id;

        var result = await _controller.AnularVenta(id);

        result.Should().BeOfType<NoContentResult>();
        var venta = await _context.Ventas.IgnoreQueryFilters().FirstAsync(v => v.Id == id);
        venta.Anulada.Should().BeTrue("RF-050");
        var stock = await _context.Stocks.FirstAsync(s => s.ProductoId == productoId);
        stock.Cantidad.Should().Be(10, "RF-050: stock restituido");
    }

    [Fact]
    public async Task Anular_YaAnulada_LanzaBusinessRule()
    {
        await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        var creada = await _controller.CreateVenta(NuevaVenta(productoId));
        var id = ((VentaDto)((CreatedAtActionResult)creada.Result!).Value!).Id;

        await _controller.AnularVenta(id);

        var act = () => _controller.AnularVenta(id);
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Anular_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.AnularVenta(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
