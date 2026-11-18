using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests del validador de ventas — Etapa 9 (RF-044, RF-041, RF-076, RF-077).
/// </summary>
public class VentaValidatorTests
{
    private static VentaCreateDto Valid() => new()
    {
        MetodoPago = "Efectivo",
        Detalles = new List<DetalleVentaCreateDto>
        {
            new() { ProductoId = 1, Cantidad = 2 }
        }
    };

    [Fact]
    public void DatosValidos_EsValido()
    {
        new VentaCreateValidator().Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Efectivo")]
    [InlineData("Débito")]
    [InlineData("Crédito")]
    [InlineData("Transferencia")]
    public void MetodosDePagoValidos_RF044_SonValidos(string metodo)
    {
        var dto = Valid();
        dto.MetodoPago = metodo;

        new VentaCreateValidator().Validate(dto).IsValid.Should().BeTrue(
            "RF-044: los cuatro métodos de pago mínimos");
    }

    [Fact]
    public void MetodoPagoInvalido_EsInvalido()
    {
        var dto = Valid();
        dto.MetodoPago = "Bitcoin";

        var result = new VentaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MetodoPago");
    }

    [Fact]
    public void MetodoPagoVacio_EsInvalido()
    {
        var dto = Valid();
        dto.MetodoPago = "";

        new VentaCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SinDetalles_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles = new List<DetalleVentaCreateDto>();

        var result = new VentaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Detalles");
    }

    [Fact]
    public void DetalleConCantidadCero_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles[0].Cantidad = 0;

        var result = new VentaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }

    [Fact]
    public void DetalleConProductoIdCero_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles[0].ProductoId = 0;

        new VentaCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }
}
