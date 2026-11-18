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
/// Tests de controller de clientes con base de datos en memoria —
/// Etapa 6 (RF-027 a RF-031, RNF-017 códigos HTTP, RNF-062).
/// </summary>
public class ClientesControllerTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly ClientesController _controller;

    public ClientesControllerTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _controller = new ClientesController(
            new ClienteService(new ClienteRepository(_context)),
            new ClienteCreateValidator(),
            new ClienteUpdateValidator());
    }

    public void Dispose() => _context.Dispose();

    // ── RF-027 / RNF-017: alta y códigos HTTP ──────────────────────────────

    [Fact]
    public async Task Create_DatosValidos_Return201Created()
    {
        var result = await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "María",
            Apellido = "Gómez",
            Documento = "27888777",
            Email = "maria@example.com"
        });

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        ((ClienteDto)created.Value!).Documento.Should().Be("27888777");
    }

    [Fact]
    public async Task Create_DatosInvalidos_Return400ValidationProblem()
    {
        var result = await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "",
            Apellido = ""
        });

        var badRequest = result.Result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    // ── RF-028 / RF-029: consulta y búsqueda ───────────────────────────────

    [Fact]
    public async Task GetClientes_Return200ConLista()
    {
        await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "Ana",
            Apellido = "Torres",
            Documento = "30111222"
        });

        var result = await _controller.GetClientes();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ClienteDto>)ok.Value!).Should().ContainSingle();
    }

    [Fact]
    public async Task Search_ConFiltros_DevuelveSoloCoincidencias()
    {
        await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "Carlos",
            Apellido = "López",
            Documento = "30555666"
        });
        await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "Ana",
            Apellido = "Torres",
            Documento = "30777888"
        });

        var result = await _controller.Search(nombre: "Carlos", documento: null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<ClienteDto>)ok.Value!).Should().ContainSingle(c => c.Documento == "30555666");
    }

    [Fact]
    public async Task GetById_Inexistente_Return404NotFound()
    {
        var result = await _controller.GetCliente(9999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── RF-030: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task Update_Existente_Return204NoContent()
    {
        var created = await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "Viejo",
            Apellido = "Apellido",
            Documento = "30000111"
        });
        var id = ((ClienteDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.UpdateCliente(id, new ClienteUpdateDto
        {
            Nombre = "Nuevo",
            Apellido = "Apellido",
            Documento = "30000111"
        });

        result.Should().BeOfType<NoContentResult>();
        (await _context.Clientes.FindAsync(id))!.Nombre.Should().Be("Nuevo");
    }

    [Fact]
    public async Task Update_DatosInvalidos_Return400ValidationProblem()
    {
        var result = await _controller.UpdateCliente(1, new ClienteUpdateDto
        {
            Nombre = "",
            Apellido = ""
        });

        var badRequest = result.Should().BeAssignableTo<ObjectResult>().Which;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Update_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.UpdateCliente(9999, new ClienteUpdateDto
        {
            Nombre = "X",
            Apellido = "Y"
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-031: baja lógica ─────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Existente_Return204NoContent()
    {
        var created = await _controller.CreateCliente(new ClienteCreateDto
        {
            Nombre = "Temporal",
            Apellido = "Cliente",
            Documento = "30999888"
        });
        var id = ((ClienteDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        var result = await _controller.DeleteCliente(id);

        result.Should().BeOfType<NoContentResult>();
        (await _context.Clientes.IgnoreQueryFilters().FirstAsync(c => c.Id == id))
            .Activo.Should().BeFalse("RF-031/RF-073: baja lógica");
    }

    [Fact]
    public async Task Delete_Inexistente_LanzaEntityNotFound()
    {
        var act = () => _controller.DeleteCliente(9999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
