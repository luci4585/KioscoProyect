using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests del validador de movimientos de stock — Etapa 7 (RF-076, RF-077).
/// </summary>
public class StockMovimientoValidatorTests
{
    private static StockMovimientoDto Valid() => new()
    {
        ProductoId = 1,
        Cantidad = 5
    };

    [Fact]
    public void DatosValidos_EsValido()
    {
        new StockMovimientoValidator().Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProductoIdCero_EsInvalido()
    {
        var dto = Valid();
        dto.ProductoId = 0;

        var result = new StockMovimientoValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductoId");
    }

    [Fact]
    public void CantidadCero_EsInvalida()
    {
        var dto = Valid();
        dto.Cantidad = 0;

        var result = new StockMovimientoValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cantidad");
    }

    [Fact]
    public void CantidadNegativa_EsInvalida()
    {
        var dto = Valid();
        dto.Cantidad = -3;

        new StockMovimientoValidator().Validate(dto).IsValid.Should().BeFalse();
    }
}
