using FluentAssertions;
using Kiosco.API.Controllers;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Excel;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Tests de controller de informes con base de datos en memoria —
/// Etapa 11 (RF-059 a RF-072, RNF-017 códigos HTTP).
/// </summary>
public class InformesControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly InformesController _controller;

    public InformesControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new InformesController(new InformeService(
            new VentaRepository(_context),
            new CajaRepository(_context),
            new IngresoMercaderiaRepository(_context),
            new StockRepository(_context),
            new ProductoRepository(_context),
            new ExcelExportAdapter()));
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedVentaDelDiaAsync()
    {
        var rol = new Rol { Nombre = $"Rol {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nombre = "Vendedor",
            Apellido = "Test",
            NombreUsuario = $"v{Guid.NewGuid():N}"[..12],
            Email = $"v-{Guid.NewGuid():N}@test.local",
            FirebaseUid = $"uid-{Guid.NewGuid():N}",
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto",
            Codigo = $"INF-{Guid.NewGuid():N}"[..9],
            RubroId = rubro.Id,
            PrecioVenta = 100,
            Costo = 60,
            StockMinimo = 1
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var venta = new Venta
        {
            Fecha = DateTime.Today.AddHours(10),
            UsuarioId = usuario.Id,
            Total = 200,
            MetodoPago = "Efectivo",
            DetallesVenta = new List<DetalleVenta>
            {
                new()
                {
                    ProductoId = producto.Id, Cantidad = 2,
                    PrecioUnitario = 100, Subtotal = 200, Producto = producto
                }
            }
        };
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta.Id;
    }

    [Fact]
    public async Task GetResumen_Return200ConLosCamposRF064()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.GetResumen(DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (ResumenDiarioDto)ok.Value!;
        dto.TotalVentas.Should().Be(200m, "RF-064");
        dto.CostoVendido.Should().Be(120m, "2 × costo 60");
        dto.Ganancia.Should().Be(80m, "RF-065");
        dto.CantidadVentas.Should().Be(1);
    }

    [Fact]
    public async Task GetResumen_SinFecha_DefaultHoy()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.GetResumen(null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((ResumenDiarioDto)ok.Value!).Fecha.Should().Be(DateTime.Today);
    }

    [Fact]
    public async Task GetVentas_Return200ConInforme()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.GetVentas(DateTime.Today, DateTime.Today, null, null, null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (InformeVentasDto)ok.Value!;
        dto.Cantidad.Should().Be(1, "RF-059");
        dto.TotalVendido.Should().Be(200m);
    }

    [Fact]
    public async Task GetGanancia_Return200()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.GetGanancia(DateTime.Today, DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((InformeGananciaDto)ok.Value!).Ganancia.Should().Be(80m, "RF-066");
    }

    [Fact]
    public async Task GetMasVendidos_Return200()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.GetMasVendidos(DateTime.Today, DateTime.Today, 5);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ProductoVendidoDto>)ok.Value!).Should().ContainSingle("RF-063");
    }

    [Fact]
    public async Task GetCaja_Return200Lista()
    {
        var result = await _controller.GetCaja(DateTime.Today, DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<CajaDto>)ok.Value!).Should().BeEmpty();
    }

    [Fact]
    public async Task Exportar_TipoValido_ReturnFileXlsx()
    {
        await SeedVentaDelDiaAsync();

        var result = await _controller.Exportar("ventas", DateTime.Today, DateTime.Today);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RNF-058");
        file.FileDownloadName.Should().StartWith("informe_ventas_").And.EndWith(".xlsx");
        file.FileContents.Should().NotBeEmpty("RF-072");
        file.FileContents[0].Should().Be((byte)0x50); // firma PK del OOXML
    }

    [Fact]
    public async Task Exportar_TipoInvalido_LanzaBusinessRule()
    {
        // Se propaga al middleware → 400, igual que el resto de las reglas
        // de negocio del sistema (RNF-020).
        var act = () => _controller.Exportar("pdf", DateTime.Today, DateTime.Today);
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Tipo de informe no soportado*");
    }
}
