using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Excel;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de informes y dashboard — Etapa 11 (RoadMap §14; RF-059 a RF-072).
/// Fórmulas: RF-065 (ganancia) y RF-067 (monto final de caja).
/// Usa el adaptador real de Excel con ClosedXML (RNF-057).
/// </summary>
public class InformeServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly InformeService _service;

    public InformeServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new InformeService(
            new VentaRepository(_context),
            new CajaRepository(_context),
            new IngresoMercaderiaRepository(_context),
            new StockRepository(_context),
            new ProductoRepository(_context),
            new ExcelExportAdapter());
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedUsuarioAsync(string nombre = "Vendedor", string apellido = "Uno")
    {
        var rol = new Rol { Nombre = $"Rol {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            NombreUsuario = $"u{Guid.NewGuid():N}"[..12],
            Email = $"u-{Guid.NewGuid():N}@test.local",
            FirebaseUid = $"uid-{Guid.NewGuid():N}",
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    private async Task<int> SeedProductoAsync(decimal costo = 60, string codigo = "INF-1")
    {
        var rubro = new Rubro { Nombre = $"Rubro {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Informe",
            Codigo = codigo,
            RubroId = rubro.Id,
            PrecioVenta = 150,
            Costo = costo,
            StockMinimo = 1
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = 100 });
        await _context.SaveChangesAsync();

        return producto.Id;
    }

    private async Task<int> SeedVentaAsync(
        int usuarioId, int productoId, int cantidad, decimal precio,
        DateTime fecha, string metodo = "Efectivo", bool anulada = false, int? clienteId = null)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        var venta = new Venta
        {
            Fecha = fecha,
            UsuarioId = usuarioId,
            ClienteId = clienteId,
            Total = precio * cantidad,
            MetodoPago = metodo,
            Anulada = anulada,
            DetallesVenta = new List<DetalleVenta>
            {
                new()
                {
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = precio,
                    Subtotal = precio * cantidad,
                    Producto = producto!
                }
            }
        };
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta.Id;
    }

    private async Task<int> SeedCajaAsync(
        int usuarioId, decimal montoInicial, DateTime fechaApertura,
        decimal ingreso = 0, decimal egreso = 0, bool abierta = false)
    {
        var caja = new Caja
        {
            FechaApertura = fechaApertura,
            MontoInicial = montoInicial,
            Abierta = abierta,
            UsuarioAperturaId = usuarioId
        };
        _context.Cajas.Add(caja);
        await _context.SaveChangesAsync();

        if (ingreso > 0)
            _context.MovimientosCaja.Add(new MovimientoCaja
            {
                CajaId = caja.Id, Fecha = fechaApertura, TipoMovimiento = "Ingreso",
                Monto = ingreso, Descripcion = "test", UsuarioId = usuarioId
            });
        if (egreso > 0)
            _context.MovimientosCaja.Add(new MovimientoCaja
            {
                CajaId = caja.Id, Fecha = fechaApertura, TipoMovimiento = "Egreso",
                Monto = egreso, Descripcion = "test", UsuarioId = usuarioId
            });
        await _context.SaveChangesAsync();
        return caja.Id;
    }

    // ── RF-064 / RF-065 / RF-067 / RF-071: resumen del día ─────────────────

    [Fact]
    public async Task GetResumenDiario_CalculaTodosLosCamposDelDia()
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync("Cajero", "Díaz");
        var productoId = await SeedProductoAsync(costo: 60);
        await SeedCajaAsync(usuarioId, montoInicial: 1000, fechaApertura: hoy.AddHours(9),
            ingreso: 500, egreso: 200, abierta: true);
        await SeedVentaAsync(usuarioId, productoId, cantidad: 2, precio: 150, fecha: hoy.AddHours(10));
        await SeedVentaAsync(usuarioId, productoId, cantidad: 1, precio: 150, fecha: hoy.AddHours(11),
            metodo: "Crédito");
        await SeedVentaAsync(usuarioId, productoId, cantidad: 5, precio: 150, fecha: hoy.AddHours(12),
            anulada: true); // no debe contar

        var resumen = await _service.GetResumenDiarioAsync(hoy);

        // RF-064
        resumen.Fecha.Should().Be(hoy);
        resumen.MontoInicialCaja.Should().Be(1000m);
        resumen.TotalIngresos.Should().Be(500m);
        resumen.TotalEgresos.Should().Be(200m);
        resumen.TotalVentas.Should().Be(450m, "2×150 + 1×150; la anulada no cuenta");
        resumen.CantidadVentas.Should().Be(2);
        resumen.ProductosVendidos.Should().Be(3, "2 + 1 unidades");
        // RF-065: ganancia = total de ventas − costo de lo vendido (3 × 60).
        resumen.CostoVendido.Should().Be(180m);
        resumen.Ganancia.Should().Be(270m, "450 − 180 (RF-065)");
        // RF-067: monto final = inicial + ingresos − egresos.
        resumen.MontoFinalCaja.Should().Be(1300m, "1000 + 500 − 200 (RF-067)");
        // RF-071
        resumen.CajaAbierta.Should().BeTrue();
        // RF-068
        resumen.UsuarioApertura.Should().Be("Cajero Díaz");
        resumen.VentasPorMetodoPago.Should().HaveCount(2);
        resumen.VentasPorMetodoPago.Single(m => m.MetodoPago == "Efectivo").Total.Should().Be(300m);
        resumen.VentasPorMetodoPago.Single(m => m.MetodoPago == "Crédito").Total.Should().Be(150m);
    }

    [Fact]
    public async Task GetResumenDiario_SinCaja_YSinVentas_DevuelveCeros()
    {
        var resumen = await _service.GetResumenDiarioAsync(DateTime.Today);

        resumen.MontoInicialCaja.Should().Be(0);
        resumen.MontoFinalCaja.Should().Be(0);
        resumen.TotalVentas.Should().Be(0);
        resumen.Ganancia.Should().Be(0);
        resumen.CajaAbierta.Should().BeFalse();
        resumen.UsuarioApertura.Should().BeNull();
        resumen.VentasPorMetodoPago.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResumenDiario_NoAfectaLasVentasDeOtroDia()
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        await SeedVentaAsync(usuarioId, productoId, 1, 150, hoy.AddDays(-1));

        var resumen = await _service.GetResumenDiarioAsync(hoy);

        resumen.TotalVentas.Should().Be(0, "RF-070: los informes son por fecha");
        resumen.CantidadVentas.Should().Be(0);
    }

    // ── RF-059 / RF-060: ventas del período con filtros ────────────────────

    [Fact]
    public async Task GetVentasPeriodo_FiltraPorFechaMetodoClienteYUsuario()
    {
        var hoy = DateTime.Today;
        var usuarioA = await SeedUsuarioAsync("Vendedor", "A");
        var usuarioB = await SeedUsuarioAsync("Vendedor", "B");
        var productoId = await SeedProductoAsync();

        var cliente = new Cliente { Nombre = "Cliente", Apellido = "Uno", Documento = "301" };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        await SeedVentaAsync(usuarioA, productoId, 1, 100, hoy.AddHours(9), "Efectivo", clienteId: cliente.Id);
        await SeedVentaAsync(usuarioB, productoId, 2, 100, hoy.AddHours(10), "Crédito");
        await SeedVentaAsync(usuarioA, productoId, 3, 100, hoy.AddDays(-1), "Efectivo"); // fuera de fecha

        // Sin filtros (solo el día): 2 ventas, 300 total.
        var general = await _service.GetVentasPeriodoAsync(hoy, hoy, null, null, null);
        general.Cantidad.Should().Be(2, "RF-059: ventas del período");
        general.TotalVendido.Should().Be(300m);

        // RF-060: método de pago.
        var porMetodo = await _service.GetVentasPeriodoAsync(hoy, hoy, null, null, "Crédito");
        porMetodo.Cantidad.Should().Be(1);
        porMetodo.TotalVendido.Should().Be(200m);

        // RF-060: usuario.
        var porUsuario = await _service.GetVentasPeriodoAsync(hoy, hoy, usuarioA, null, null);
        porUsuario.Cantidad.Should().Be(1);
        porUsuario.Ventas.Single().UsuarioNombre.Should().Be("Vendedor A");

        // RF-060: cliente.
        var porCliente = await _service.GetVentasPeriodoAsync(hoy, hoy, null, cliente.Id, null);
        porCliente.Cantidad.Should().Be(1);

        // RF-066: ganancia del informe de ventas (100 + 200 = 300;
        // costo 3 unidades × 60 = 180 → 300 − 180 = 120).
        general.Ganancia.Should().Be(120m, "300 − 180 (RF-065)");
    }

    [Fact]
    public async Task GetVentasPeriodo_PeriodoInvalido_LanzaBusinessRule()
    {
        var act = () => _service.GetVentasPeriodoAsync(DateTime.Today, DateTime.Today.AddDays(-2), null, null, null);
        await act.Should().ThrowAsync<BusinessRuleException>("el período no puede estar invertido");
    }

    // ── RF-066: ganancia por período ───────────────────────────────────────

    [Fact]
    public async Task GetGananciaPeriodo_AplicaLaFormulaRF065()
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync(costo: 70);
        await SeedVentaAsync(usuarioId, productoId, 4, 150, hoy.AddHours(9)); // total 600, costo 280

        var ganancia = await _service.GetGananciaPeriodoAsync(hoy, hoy);

        ganancia.TotalVendido.Should().Be(600m);
        ganancia.CostoVendido.Should().Be(280m, "4 × costo vigente 70");
        ganancia.Ganancia.Should().Be(320m, "RF-065: 600 − 280");
    }

    // ── RF-063: productos más vendidos ─────────────────────────────────────

    [Fact]
    public async Task GetMasVendidos_OrdenaPorUnidadesYRespetaElTop()
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync();
        var productoA = await SeedProductoAsync(codigo: "MAS-A");
        var productoB = await SeedProductoAsync(codigo: "MAS-B");
        var productoC = await SeedProductoAsync(codigo: "MAS-C");

        await SeedVentaAsync(usuarioId, productoA, 5, 100, hoy.AddHours(9));
        await SeedVentaAsync(usuarioId, productoA, 2, 100, hoy.AddHours(10));
        await SeedVentaAsync(usuarioId, productoB, 3, 100, hoy.AddHours(11));
        await SeedVentaAsync(usuarioId, productoC, 1, 100, hoy.AddHours(12));

        var ranking = (await _service.GetMasVendidosAsync(hoy, hoy, null)).ToList();

        ranking.Should().HaveCount(3);
        ranking[0].ProductoCodigo.Should().Be("MAS-A", "RF-063: mayor cantidad de unidades");
        ranking[0].UnidadesVendidas.Should().Be(7);
        ranking[1].ProductoCodigo.Should().Be("MAS-B");
        ranking[1].UnidadesVendidas.Should().Be(3);

        var top2 = (await _service.GetMasVendidosAsync(hoy, hoy, 2)).ToList();
        top2.Should().HaveCount(2, "respeta el top solicitado");
    }

    // ── RF-062: caja por período ───────────────────────────────────────────

    [Fact]
    public async Task GetCajaPeriodo_DevuelveCajasDelPeríodoConTotales()
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync();
        await SeedCajaAsync(usuarioId, 1000, hoy.AddHours(9), ingreso: 400, egreso: 100);
        await SeedCajaAsync(usuarioId, 500, hoy.AddDays(-5), ingreso: 50);

        var cajas = (await _service.GetCajaPeriodoAsync(hoy, hoy)).ToList();

        cajas.Should().HaveCount(1, "RF-062: solo las cajas del período");
        cajas[0].TotalIngresos.Should().Be(400m);
        cajas[0].TotalEgresos.Should().Be(100m);
        cajas[0].SaldoCalculado.Should().Be(1300m, "1000 + 400 − 100");
    }

    // ── RF-072 / RNF-057: exportación Excel ────────────────────────────────

    [Theory]
    [InlineData("ventas")]
    [InlineData("productos")]
    [InlineData("stock")]
    [InlineData("movimientos-caja")]
    [InlineData("ingresos-mercaderia")]
    [InlineData("informes-diarios")]
    [InlineData("ganancias")]
    public async Task ExportarAsync_LosSieteTipos_GeneranUnXlsxValido(string tipo)
    {
        var hoy = DateTime.Today;
        var usuarioId = await SeedUsuarioAsync();
        var productoId = await SeedProductoAsync();
        await SeedVentaAsync(usuarioId, productoId, 1, 150, hoy.AddHours(9));
        await SeedCajaAsync(usuarioId, 1000, hoy.AddHours(8), ingreso: 150);

        var bytes = await _service.ExportarAsync(tipo, hoy, hoy);

        bytes.Should().NotBeEmpty($"RF-072: debe generar el Excel '{tipo}'");
        // Firma ZIP/OOXML: "PK" (RNF-057: archivo Excel real, no vacío).
        bytes[0].Should().Be((byte)0x50);
        bytes[1].Should().Be((byte)0x4B);
    }

    [Fact]
    public async Task ExportarAsync_TipoNoSoportado_LanzaBusinessRule()
    {
        var act = () => _service.ExportarAsync("pdf", DateTime.Today, DateTime.Today);
        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-072: solo formatos soportados; el listado indica los tipos válidos");
    }
}
