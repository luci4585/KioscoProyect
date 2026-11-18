using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de validadores de clientes — Etapa 6 (RoadMap §9; RF-076, RF-077).
/// </summary>
public class ClienteValidatorTests
{
    private static ClienteCreateDto ValidCreate() => new()
    {
        Nombre = "Juan",
        Apellido = "Pérez",
        Documento = "30111222",
        Email = "juan@example.com"
    };

    private static ClienteUpdateDto ValidUpdate() => new()
    {
        Nombre = "Juan",
        Apellido = "Pérez",
        Documento = "30111222",
        Email = "juan@example.com"
    };

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_DatosValidos_EsValido()
    {
        var result = new ClienteCreateValidator().Validate(ValidCreate());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_NombreVacio_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Nombre = "";

        var result = new ClienteCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void Create_ApellidoVacio_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Apellido = "";

        var result = new ClienteCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Apellido");
    }

    [Fact]
    public void Create_NombreMayorA100_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Nombre = new string('A', 101);

        new ClienteCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_EmailInvalido_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Email = "no-es-un-email";

        new ClienteCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_SinEmail_EsValido()
    {
        var dto = ValidCreate();
        dto.Email = null;

        new ClienteCreateValidator().Validate(dto).IsValid.Should().BeTrue(
            "el email es opcional (RF-027 no lo exige)");
    }

    // ── Update (mismas reglas que Create — patrón Rubros/Productos) ─────────

    [Fact]
    public void Update_DatosValidos_EsValido()
    {
        var result = new ClienteUpdateValidator().Validate(ValidUpdate());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_NombreVacio_EsInvalido()
    {
        var dto = ValidUpdate();
        dto.Nombre = "";

        var result = new ClienteUpdateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void Update_ApellidoVacio_EsInvalido()
    {
        var dto = ValidUpdate();
        dto.Apellido = "";

        new ClienteUpdateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Update_EmailInvalido_EsInvalido()
    {
        var dto = ValidUpdate();
        dto.Email = "no-es-un-email";

        new ClienteUpdateValidator().Validate(dto).IsValid.Should().BeFalse();
    }
}
