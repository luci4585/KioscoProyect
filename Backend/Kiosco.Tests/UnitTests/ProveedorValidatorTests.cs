using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Validators;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de validadores de proveedores — Etapa 6 (RoadMap §9; RF-076, RF-077).
/// </summary>
public class ProveedorValidatorTests
{
    private static ProveedorCreateDto ValidCreate() => new()
    {
        RazonSocial = "Distribuidora Sur",
        Cuit = "30711223344",
        Email = "contacto@distribuidora.com"
    };

    private static ProveedorUpdateDto ValidUpdate() => new()
    {
        RazonSocial = "Distribuidora Sur",
        Cuit = "30711223344",
        Email = "contacto@distribuidora.com"
    };

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_DatosValidos_EsValido()
    {
        var result = new ProveedorCreateValidator().Validate(ValidCreate());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_RazonSocialVacia_EsInvalida()
    {
        var dto = ValidCreate();
        dto.RazonSocial = "";

        var result = new ProveedorCreateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RazonSocial");
    }

    [Fact]
    public void Create_RazonSocialMayorA150_EsInvalida()
    {
        var dto = ValidCreate();
        dto.RazonSocial = new string('A', 151);

        new ProveedorCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_CuitMayorA11_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Cuit = new string('9', 12);

        new ProveedorCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_EmailInvalido_EsInvalido()
    {
        var dto = ValidCreate();
        dto.Email = "no-es-un-email";

        new ProveedorCreateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_OpcionalesVacios_EsValido()
    {
        var dto = ValidCreate();
        dto.Cuit = null;
        dto.Email = null;

        new ProveedorCreateValidator().Validate(dto).IsValid.Should().BeTrue(
            "CUIT y email son opcionales (RF-032)");
    }

    // ── Update (mismas reglas que Create — patrón Rubros/Productos) ─────────

    [Fact]
    public void Update_DatosValidos_EsValido()
    {
        var result = new ProveedorUpdateValidator().Validate(ValidUpdate());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_RazonSocialVacia_EsInvalida()
    {
        var dto = ValidUpdate();
        dto.RazonSocial = "";

        var result = new ProveedorUpdateValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RazonSocial");
    }

    [Fact]
    public void Update_CuitMayorA11_EsInvalido()
    {
        var dto = ValidUpdate();
        dto.Cuit = new string('9', 12);

        new ProveedorUpdateValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Update_EmailInvalido_EsInvalido()
    {
        var dto = ValidUpdate();
        dto.Email = "no-es-un-email";

        new ProveedorUpdateValidator().Validate(dto).IsValid.Should().BeFalse();
    }
}
