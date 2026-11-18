using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests del validador de ingresos de mercadería — Etapa 8 (RF-076, RF-077).
/// </summary>
public class IngresoMercaderiaValidatorTests
{
    private static IngresoMercaderiaCreateDto Valid() => new()
    {
        ProveedorId = 1,
        Detalles = new List<DetalleIngresoMercaderiaCreateDto>
        {
            new() { ProductoId = 1, Cantidad = 10, CostoUnitario = 80.5m }
        }
    };

    [Fact]
    public void DatosValidos_EsValido()
    {
        new IngresoMercaderiaCreateValidator().Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProveedorIdCero_EsInvalido()
    {
        var dto = Valid();
        dto.ProveedorId = 0;

        var result = new IngresoMercaderiaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProveedorId");
    }

    [Fact]
    public void SinDetalles_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles = new List<DetalleIngresoMercaderiaCreateDto>();

        var result = new IngresoMercaderiaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Detalles");
    }

    [Fact]
    public void DetalleConCantidadCero_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles[0].Cantidad = 0;

        var result = new IngresoMercaderiaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }

    [Fact]
    public void DetalleConCostoNegativo_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles[0].CostoUnitario = -1;

        var result = new IngresoMercaderiaCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("CostoUnitario"));
    }

    [Fact]
    public void DetalleConProductoIdCero_EsInvalido()
    {
        var dto = Valid();
        dto.Detalles[0].ProductoId = 0;

        new IngresoMercaderiaCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }
}
