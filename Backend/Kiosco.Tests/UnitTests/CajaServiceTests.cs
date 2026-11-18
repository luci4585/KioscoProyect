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
/// Tests de servicio de caja — Etapa 10 (RoadMap §13; RF-051 a RF-058).
/// Fórmula central (RoadMap §13): Monto inicial + Ingresos − Egresos = Monto final.
/// Sobre base de datos en memoria (SDD §30.10).
/// </summary>
public class CajaServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly CajaService _service;

    public CajaServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _service = new CajaService(
            new CajaRepository(_context),
            new MovimientoCajaRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedUsuarioAsync()
    {
        var rol = new Rol { Nombre = $"Rol {Guid.NewGuid():N}"[..14], Descripcion = "t" };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nombre = "Cajero",
            Apellido = "De Prueba",
            NombreUsuario = $"caj{Guid.NewGuid():N}"[..12],
            Email = $"caj-{Guid.NewGuid():N}@test.local",
            FirebaseUid = $"uid-{Guid.NewGuid():N}",
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    private async Task<int> SeedCajaAbiertaAsync(int usuarioId, decimal montoInicial = 1000)
    {
        var caja = new Caja
        {
            FechaApertura = DateTime.UtcNow,
            MontoInicial = montoInicial,
            Abierta = true,
            UsuarioAperturaId = usuarioId
        };
        _context.Cajas.Add(caja);
        await _context.SaveChangesAsync();
        return caja.Id;
    }

    // ── RF-051: apertura ────────────────────────────────────────────────────

    [Fact]
    public async Task AbrirAsync_CreaLaCajaConMontoInicial()
    {
        var usuarioId = await SeedUsuarioAsync();

        var caja = await _service.AbrirAsync(new CajaAperturaDto { MontoInicial = 500 }, usuarioId);

        caja.Id.Should().BeGreaterThan(0);
        caja.MontoInicial.Should().Be(500m, "RF-051: monto inicial disponible");
        caja.Abierta.Should().BeTrue();
        caja.TotalIngresos.Should().Be(0);
        caja.SaldoCalculado.Should().Be(500m);
    }

    [Fact]
    public async Task AbrirAsync_YaHayCajaAbierta_LanzaBusinessRule()
    {
        var usuarioId = await SeedUsuarioAsync();
        await SeedCajaAbiertaAsync(usuarioId);

        var act = () => _service.AbrirAsync(new CajaAperturaDto { MontoInicial = 100 }, usuarioId);

        await act.Should().ThrowAsync<BusinessRuleException>(
            "solo puede haber una caja abierta");
    }

    [Fact]
    public async Task GetCajaAbiertaAsync_SinCaja_DevuelveNull()
    {
        (await _service.GetCajaAbiertaAsync()).Should().BeNull();
    }

    // ── RF-052: movimientos ─────────────────────────────────────────────────

    [Fact]
    public async Task RegistrarMovimientoAsync_CreaIngresoConSusDatos()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId);

        var movimiento = await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso",
            Monto = 250,
            Descripcion = "Proveedor"
        }, usuarioId);

        movimiento.Id.Should().BeGreaterThan(0);
        movimiento.TipoMovimiento.Should().Be("Ingreso", "RF-052");
        movimiento.Monto.Should().Be(250m, "RF-052");
        movimiento.Descripcion.Should().Be("Proveedor", "RF-052");
        movimiento.Fecha.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1), "RF-052: fecha y hora");
        movimiento.UsuarioNombre.Should().Be("Cajero De Prueba", "RF-052: usuario responsable");
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_SinCajaAbierta_LanzaBusinessRule()
    {
        var usuarioId = await SeedUsuarioAsync();

        var act = () => _service.RegistrarMovimientoAsync(1, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso",
            Monto = 10
        }, usuarioId);

        await act.Should().ThrowAsync<BusinessRuleException>("no hay caja abierta");
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_CajaDistintaALaAbierta_LanzaBusinessRule()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId);

        var act = () => _service.RegistrarMovimientoAsync(cajaId + 999, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso",
            Monto = 10
        }, usuarioId);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    // ── RF-055: resumen con la fórmula del RoadMap §13 ──────────────────────

    [Fact]
    public async Task GetCajaAbiertaAsync_Resumen_MontoInicialMasIngresosMenosEgresos()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId, montoInicial: 1000);

        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 500, Descripcion = "Ventas del día"
        }, usuarioId);
        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso", Monto = 200, Descripcion = "Insumos"
        }, usuarioId);

        var resumen = await _service.GetCajaAbiertaAsync();

        resumen.Should().NotBeNull();
        resumen!.MontoInicial.Should().Be(1000m);
        resumen.TotalIngresos.Should().Be(500m, "RF-055");
        resumen.TotalEgresos.Should().Be(200m, "RF-055");
        resumen.SaldoCalculado.Should().Be(1300m,
            "RoadMap §13: 1000 + 500 − 200 = 1300");
        resumen.Movimientos.Should().HaveCount(2, "RF-054: movimientos consultables");
    }

    // ── RF-056 / RF-057: cierre y diferencia ────────────────────────────────

    [Fact]
    public async Task CerrarAsync_SaldoCorrecto_DiferenciaCero()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId, montoInicial: 1000);
        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 500, Descripcion = "v"
        }, usuarioId);
        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso", Monto = 200, Descripcion = "e"
        }, usuarioId);

        // 1000 + 500 − 200 = 1300 → informado 1300 → diferencia 0.
        var caja = await _service.CerrarAsync(cajaId, new CajaCierreDto { MontoFinalInformado = 1300 }, usuarioId);

        caja.Abierta.Should().BeFalse("RF-056");
        caja.FechaCierre.Should().NotBeNull();
        caja.MontoFinal.Should().Be(1300m, "RF-056: saldo del período informado");
        caja.Diferencia.Should().Be(0m, "RF-057: sin diferencias");
    }

    [Fact]
    public async Task CerrarAsync_MontoInformadoMenor_DiferenciaNegativa()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId, montoInicial: 1000);
        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 500, Descripcion = "v"
        }, usuarioId);

        // Saldo calculado 1500; informado 1450 → diferencia −50.
        var caja = await _service.CerrarAsync(cajaId, new CajaCierreDto { MontoFinalInformado = 1450 }, usuarioId);

        caja.Diferencia.Should().Be(-50m, "RF-057: diferencia identificada");
    }

    [Fact]
    public async Task CerrarAsync_YaCerrada_LanzaBusinessRule()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId);

        await _service.CerrarAsync(cajaId, new CajaCierreDto { MontoFinalInformado = 1000 }, usuarioId);

        var act = () => _service.CerrarAsync(cajaId, new CajaCierreDto { MontoFinalInformado = 1000 }, usuarioId);
        await act.Should().ThrowAsync<BusinessRuleException>("no se cierra dos veces");
    }

    [Fact]
    public async Task CerrarAsync_CajaInexistente_LanzaEntityNotFound()
    {
        var act = () => _service.CerrarAsync(9999, new CajaCierreDto { MontoFinalInformado = 0 }, 1);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-054 / RF-058: consulta e historial ───────────────────────────────

    [Fact]
    public async Task GetWithDetailsAsync_DevuelveLosMovimientos()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId);
        await _service.RegistrarMovimientoAsync(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 100, Descripcion = "x"
        }, usuarioId);

        var detalle = await _service.GetWithDetailsAsync(cajaId);

        detalle.Should().NotBeNull();
        detalle!.Movimientos.Should().ContainSingle("RF-054: movimientos consultables");
        detalle.Movimientos[0].Descripcion.Should().Be("x");
        detalle.UsuarioAperturaNombre.Should().Be("Cajero De Prueba");
    }

    [Fact]
    public async Task GetWithDetailsAsync_Inexistente_DevuelveNull()
    {
        (await _service.GetWithDetailsAsync(9999)).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_DevuelveElHistorialDeCajas()
    {
        var usuarioId = await SeedUsuarioAsync();
        var cajaId = await SeedCajaAbiertaAsync(usuarioId);
        await _service.CerrarAsync(cajaId, new CajaCierreDto { MontoFinalInformado = 1000 }, usuarioId);
        await _service.AbrirAsync(new CajaAperturaDto { MontoInicial = 50 }, usuarioId);

        var historial = (await _service.GetAllAsync()).ToList();

        historial.Should().HaveCount(2, "RF-058: aperturas y cierres anteriores");
        historial[0].Abierta.Should().BeTrue("ordenado por apertura descendente");
        historial[1].Abierta.Should().BeFalse();
        historial[1].FechaCierre.Should().NotBeNull();
    }
}
