using FluentAssertions;
using Kiosco.API.Controllers;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Application.Validators;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Tests de controller de proveedores con base de datos en memoria —
/// Etapa 6 (RF-032 a RF-035, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class ProveedoresControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly ProveedoresController _controller;

    public ProveedoresControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new ProveedoresController(
            new ProveedorService(new ProveedorRepository(_context)),
            new ProveedorCreateValidator(),
            new ProveedorUpdateValidator());
    }

    public void Dispose() => _context.Dispose();

    // ── RF-032 / RNF-017: alta y códigos HTTP ──────────────────────────────

    [Fact]
    public async Task Create_DatosValidos_Return201Created()
    {
        var result = await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Alimentos del Norte",
            Cuit = "30999888777",
            Email = "norte@example.com"
        });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        ((ProveedorDto)created.Value!).RazonSocial.Should().Be("Alimentos del Norte");
    }

    [Fact]
    public async Task Create_DatosInvalidos_Return400ValidationProblem()
    {
        var result = await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = ""
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_CuitDuplicado_LanzaBusinessRule()
    {
        await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Primero",
            Cuit = "30123456789"
        });

        var act = () => _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Segundo",
            Cuit = "30123456789"
        });

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-077: unicidad de CUIT");
    }

    // ── RF-033: consulta ────────────────────────────────────────────────────

    [Fact]
    public async Task GetProveedores_Return200ConLista()
    {
        await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Distribuidora Sur"
        });

        var result = await _controller.GetProveedores();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ProveedorDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task Search_ConFiltros_DevuelveSoloCoincidencias()
    {
        await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Bebidas Quilmes",
            Cuit = "30333000333"
        });
        await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Snacks Andes",
            Cuit = "30444000444"
        });

        var result = await _controller.Search(razonSocial: "Bebidas", cuit: null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ProveedorDto>)ok.Value!).Should()
            .ContainSingle(p => p.RazonSocial == "Bebidas Quilmes");
    }

    [Fact]
    public async Task GetById_Inexistente_Return404NotFound()
    {
        var result = await _controller.GetProveedor(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── RF-034: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task Update_Existente_Return204NoContent()
    {
        var created = await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Vieja Razón",
            Cuit = "30000222333"
        });
        var id = ((ProveedorDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.UpdateProveedor(id, new ProveedorUpdateDto
        {
            RazonSocial = "Nueva Razón",
            Cuit = "30000222333"
        });

        result.Should().BeOfType<NoContentResult>();
        (await _context.Proveedores.FindAsync(id))!.RazonSocial.Should().Be("Nueva Razón");
    }

    [Fact]
    public async Task Update_DatosInvalidos_Return400ValidationProblem()
    {
        var result = await _controller.UpdateProveedor(1, new ProveedorUpdateDto
        {
            RazonSocial = ""
        });

        var badRequest = result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Update_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.UpdateProveedor(9999, new ProveedorUpdateDto
        {
            RazonSocial = "X"
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-035: baja lógica ─────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Existente_Return204NoContent()
    {
        var created = await _controller.CreateProveedor(new ProveedorCreateDto
        {
            RazonSocial = "Temporal",
            Cuit = "30777666555"
        });
        var id = ((ProveedorDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.DeleteProveedor(id);

        result.Should().BeOfType<NoContentResult>();
        (await _context.Proveedores.IgnoreQueryFilters().FirstAsync(p => p.Id == id))
            .Activo.Should().BeFalse("RF-035/RF-073: baja lógica");
    }

    [Fact]
    public async Task Delete_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.DeleteProveedor(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
