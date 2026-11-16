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
/// Tests de controller de rubros con base de datos en memoria — Etapa 4
/// (RF-009 a RF-012, RNF-017 códigos HTTP, RNF-062 pruebas de controllers).
/// </summary>
public class RubrosControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly RubrosController _controller;

    public RubrosControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new RubrosController(
            new RubroService(new RubroRepository(_context)),
            new RubroCreateValidator(),
            new RubroUpdateValidator());
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Create_DatosValidos_Return201Created()
    {
        var result = await _controller.CreateRubro(new RubroCreateDto
        {
            Nombre = "Golosinas",
            Descripcion = "Bombones y caramelos"
        });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        created.ActionName.Should().Be(nameof(RubrosController.GetRubro));

        var dto = created.Value.Should().BeOfType<RubroDto>().Subject;
        dto.Nombre.Should().Be("Golosinas");
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Create_NombreVacio_Return400ValidationProblem()
    {
        var result = await _controller.CreateRubro(new RubroCreateDto { Nombre = "" });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_NombreDuplicado_LanzaBusinessRule()
    {
        await _controller.CreateRubro(new RubroCreateDto { Nombre = "Snacks" });

        var act = () => _controller.CreateRubro(new RubroCreateDto { Nombre = "Snacks" });

        await act.Should().ThrowAsync<BusinessRuleException>(
            "el middleware la convertirá en 400 (RNF-020)");
    }

    [Fact]
    public async Task GetById_RubroExistente_Return200Ok()
    {
        var created = await _controller.CreateRubro(new RubroCreateDto { Nombre = "Limpieza" });
        var id = ((RubroDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.GetRubro(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ((RubroDto)ok.Value!).Nombre.Should().Be("Limpieza");
    }

    [Fact]
    public async Task GetById_RubroInexistente_Return404NotFound()
    {
        var result = await _controller.GetRubro(9999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_RubroExistente_Return204NoContent()
    {
        var created = await _controller.CreateRubro(new RubroCreateDto { Nombre = "Viejo" });
        var id = ((RubroDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.UpdateRubro(id, new RubroUpdateDto
        {
            Nombre = "Actualizado",
            Descripcion = "Nueva descripción"
        });

        result.Should().BeOfType<NoContentResult>();
        (await _context.Rubros.FindAsync(id))!.Nombre.Should().Be("Actualizado");
    }

    [Fact]
    public async Task Update_DatosInvalidos_Return400ValidationProblem()
    {
        var created = await _controller.CreateRubro(new RubroCreateDto { Nombre = "Base" });
        var id = ((RubroDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.UpdateRubro(id, new RubroUpdateDto { Nombre = "" });

        var badRequest = result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Update_RubroInexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.UpdateRubro(9999, new RubroUpdateDto { Nombre = "X" });
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Delete_RubroExistente_Return204NoContent()
    {
        var created = await _controller.CreateRubro(new RubroCreateDto { Nombre = "Temporal" });
        var id = ((RubroDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.DeleteRubro(id);

        result.Should().BeOfType<NoContentResult>();
        (await _context.Rubros.IgnoreQueryFilters().FirstAsync(r => r.Id == id))
            .Activo.Should().BeFalse("RF-012: la baja debe ser lógica");
    }

    [Fact]
    public async Task Delete_RubroInexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.DeleteRubro(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
