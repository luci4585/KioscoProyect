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
/// Tests de controller de caja con base de datos en memoria —
/// Etapa 10 (RF-051 a RF-058, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class CajasControllerTests : IDisposable
{
    private const string FirebaseUid = "uid-caja-test";

    private readonly KioscoDbContext _context;
    private readonly CajasController _controller;

    public CajasControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new CajasController(
            new CajaService(new CajaRepository(_context), new MovimientoCajaRepository(_context)),
            new CajaAperturaValidator(),
            new CajaCierreValidator(),
            new MovimientoCajaCreateValidator(),
            new UsuarioService(new UsuarioRepository(_context), new RolRepository(_context)))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
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
            Nombre = "Cajero",
            Apellido = "De Prueba",
            NombreUsuario = $"caj{Guid.NewGuid():N}"[..12],
            Email = $"caj-{Guid.NewGuid():N}@test.local",
            FirebaseUid = FirebaseUid,
            RolId = rol.Id
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    // ── RF-051: apertura ────────────────────────────────────────────────────

    [Fact]
    public async Task Abrir_DatosValidos_Return201Created()
    {
        await SeedUsuarioAsync();

        var result = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 700 });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        ((CajaDto)created.Value!).MontoInicial.Should().Be(700m, "RF-051");
    }

    [Fact]
    public async Task Abrir_MontoNegativo_Return400ValidationProblem()
    {
        await SeedUsuarioAsync();

        var result = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = -1 });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Abrir_SinUsuarioInterno_ReturnUnauthorized()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 100 });

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Abrir_YaHayCajaAbierta_LanzaBusinessRule()
    {
        await SeedUsuarioAsync();
        await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 100 });

        var act = () => _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 200 });

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    // ── RF-055: caja abierta ────────────────────────────────────────────────

    [Fact]
    public async Task GetCajaAbierta_SinCaja_Return200ConBodyNull()
    {
        var result = await _controller.GetCajaAbierta();

        var json = result.Result.Should().BeOfType<JsonResult>().Subject;
        json.StatusCode.Should().Be(200, "sin caja abierta debe ser 200 con null, no 204");
        json.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetCajaAbierta_ConCaja_Return200ConResumen()
    {
        await SeedUsuarioAsync();
        await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 500 });

        var result = await _controller.GetCajaAbierta();

        var json = result.Result.Should().BeOfType<JsonResult>().Subject;
        json.StatusCode.Should().Be(200);
        ((CajaDto)json.Value!).SaldoCalculado.Should().Be(500m, "RF-055");
    }

    // ── RF-052: movimientos ─────────────────────────────────────────────────

    [Fact]
    public async Task RegistrarMovimiento_DatosValidos_Return200()
    {
        await SeedUsuarioAsync();
        var abierta = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 1000 });
        var cajaId = ((CajaDto)((CreatedAtActionResult)abierta.Result!).Value!).Id;

        var result = await _controller.RegistrarMovimiento(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso",
            Monto = 250,
            Descripcion = "Cobro"
        });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((MovimientoCajaDto)ok.Value!).Monto.Should().Be(250m, "RF-052");
    }

    [Fact]
    public async Task RegistrarMovimiento_DatosInvalidos_Return400()
    {
        await SeedUsuarioAsync();

        var result = await _controller.RegistrarMovimiento(1, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ajuste",
            Monto = 100
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    // ── RF-056 / RF-057: cierre ─────────────────────────────────────────────

    [Fact]
    public async Task Cerrar_ConSaldoCorrecto_Return200ConDiferencia()
    {
        await SeedUsuarioAsync();
        var abierta = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 1000 });
        var cajaId = ((CajaDto)((CreatedAtActionResult)abierta.Result!).Value!).Id;
        await _controller.RegistrarMovimiento(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 300, Descripcion = "v"
        });

        // 1000 + 300 = 1300; informado 1280 → diferencia −20 (RF-057).
        var result = await _controller.CerrarCaja(cajaId, new CajaCierreDto { MontoFinalInformado = 1280 });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (CajaDto)ok.Value!;
        dto.Abierta.Should().BeFalse("RF-056");
        dto.Diferencia.Should().Be(-20m, "RF-057");
        dto.SaldoCalculado.Should().Be(1300m, "RoadMap §13: 1000 + 300 − 0");
    }

    [Fact]
    public async Task Cerrar_MontoNegativo_Return400()
    {
        await SeedUsuarioAsync();

        var result = await _controller.CerrarCaja(1, new CajaCierreDto { MontoFinalInformado = -1 });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Cerrar_CajaInexistente_LanzaEntityNotFound()
    {
        await SeedUsuarioAsync();

        var act = () => _controller.CerrarCaja(9999, new CajaCierreDto { MontoFinalInformado = 0 });
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-054 / RF-058: consulta e historial ───────────────────────────────

    [Fact]
    public async Task GetCajas_Return200Historial()
    {
        await SeedUsuarioAsync();
        await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 100 });

        var result = await _controller.GetCajas();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<CajaDto>)ok.Value!).Should().ContainSingle("RF-058");
    }

    [Fact]
    public async Task GetCaja_Existente_Return200ConMovimientos()
    {
        await SeedUsuarioAsync();
        var abierta = await _controller.AbrirCaja(new CajaAperturaDto { MontoInicial = 100 });
        var cajaId = ((CajaDto)((CreatedAtActionResult)abierta.Result!).Value!).Id;
        await _controller.RegistrarMovimiento(cajaId, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso", Monto = 40, Descripcion = "Insumos"
        });

        var result = await _controller.GetCaja(cajaId);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = (CajaDto)ok.Value!;
        dto.Movimientos.Should().ContainSingle("RF-054");
        dto.TotalEgresos.Should().Be(40m, "RF-055");
    }

    [Fact]
    public async Task GetCaja_Inexistente_Return404()
    {
        var result = await _controller.GetCaja(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
