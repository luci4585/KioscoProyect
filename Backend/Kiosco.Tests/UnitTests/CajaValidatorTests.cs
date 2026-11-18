using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de validadores de caja — Etapa 10 (RF-076, RF-077).
/// </summary>
public class CajaValidatorTests
{
    // ── Apertura (RF-051) ───────────────────────────────────────────────────

    [Fact]
    public void Apertura_MontoValido_EsValido()
    {
        var result = new CajaAperturaValidator().Validate(
            new CajaAperturaDto { MontoInicial = 1000 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Apertura_MontoCero_EsValido()
    {
        new CajaAperturaValidator().Validate(
            new CajaAperturaDto { MontoInicial = 0 }).IsValid.Should().BeTrue(
            "el monto inicial puede ser cero");
    }

    [Fact]
    public void Apertura_MontoNegativo_EsInvalido()
    {
        var result = new CajaAperturaValidator().Validate(
            new CajaAperturaDto { MontoInicial = -1 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MontoInicial");
    }

    // ── Cierre (RF-057) ─────────────────────────────────────────────────────

    [Fact]
    public void Cierre_MontoValido_EsValido()
    {
        new CajaCierreValidator().Validate(
            new CajaCierreDto { MontoFinalInformado = 1300 }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Cierre_MontoNegativo_EsInvalido()
    {
        var result = new CajaCierreValidator().Validate(
            new CajaCierreDto { MontoFinalInformado = -5 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MontoFinalInformado");
    }

    // ── Movimientos (RF-052) ────────────────────────────────────────────────

    [Theory]
    [InlineData("Ingreso")]
    [InlineData("Egreso")]
    public void Movimiento_TipoValido_EsValido(string tipo)
    {
        var result = new MovimientoCajaCreateValidator().Validate(
            new MovimientoCajaCreateDto { TipoMovimiento = tipo, Monto = 100 });

        result.IsValid.Should().BeTrue("RF-052: ingresos y egresos");
    }

    [Fact]
    public void Movimiento_TipoInvalido_EsInvalido()
    {
        var result = new MovimientoCajaCreateValidator().Validate(
            new MovimientoCajaCreateDto { TipoMovimiento = "Ajuste", Monto = 100 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TipoMovimiento");
    }

    [Fact]
    public void Movimiento_MontoCero_EsInvalido()
    {
        var result = new MovimientoCajaCreateValidator().Validate(
            new MovimientoCajaCreateDto { TipoMovimiento = "Ingreso", Monto = 0 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Monto");
    }

    [Fact]
    public void Movimiento_MontoNegativo_EsInvalido()
    {
        new MovimientoCajaCreateValidator().Validate(
            new MovimientoCajaCreateDto { TipoMovimiento = "Egreso", Monto = -10 })
            .IsValid.Should().BeFalse();
    }
}
