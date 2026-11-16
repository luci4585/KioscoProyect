using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de validación de entrada de rubros — Etapa 4 (RF-076, RF-077, RNF-049, RNF-050).
/// </summary>
public class RubroValidatorTests
{
    private readonly RubroCreateValidator _createValidator = new();
    private readonly RubroUpdateValidator _updateValidator = new();

    [Fact]
    public void Create_NombreVacio_EsInvalido()
    {
        var result = _createValidator.Validate(new RubroCreateDto { Nombre = "", Descripcion = "x" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void Create_NombreMayorA50_EsInvalido()
    {
        var result = _createValidator.Validate(new RubroCreateDto
        {
            Nombre = new string('A', 51)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Nombre" && e.ErrorMessage.Contains("50"));
    }

    [Fact]
    public void Create_DescripcionMayorA200_EsInvalida()
    {
        var result = _createValidator.Validate(new RubroCreateDto
        {
            Nombre = "Válido",
            Descripcion = new string('B', 201)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    [Fact]
    public void Create_DatosValidos_EsValido()
    {
        var result = _createValidator.Validate(new RubroCreateDto
        {
            Nombre = "Bebidas",
            Descripcion = "Gaseosas, jugos, aguas"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_MismasReglasQueCreate()
    {
        _updateValidator.Validate(new RubroUpdateDto { Nombre = "" }).IsValid.Should().BeFalse();
        _updateValidator.Validate(new RubroUpdateDto { Nombre = new string('A', 51) })
            .IsValid.Should().BeFalse();
        _updateValidator.Validate(new RubroUpdateDto
        {
            Nombre = "Válido",
            Descripcion = new string('B', 201)
        }).IsValid.Should().BeFalse();
        _updateValidator.Validate(new RubroUpdateDto { Nombre = "Válido" })
            .IsValid.Should().BeTrue();
    }
}
