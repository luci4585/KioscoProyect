using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de servicio de productos — Etapa 5 (RoadMap §8; RF-013 a RF-020,
/// RF-076/RF-077). Sobre base de datos en memoria y storage falso
/// (SDD §30.10).
/// </summary>
public class ProductoServiceTests : IDisposable
{
    private readonly KioscoDbContext _context;
    private readonly FakeStorageService _storage;
    private readonly ProductoService _service;

    public ProductoServiceTests()
    {
        var options = new DbContextOptionsBuilder<KioscoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new KioscoDbContext(options);
        _storage = new FakeStorageService();
        _service = new ProductoService(
            new ProductoRepository(_context),
            new RubroRepository(_context),
            new StockRepository(_context),
            _storage);
    }

    public void Dispose() => _context.Dispose();

    private async Task<int> SeedRubroAsync(string nombre = "Rubro Test")
    {
        var rubro = new Rubro { Nombre = nombre, Descripcion = "Para tests" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();
        return rubro.Id;
    }

    private async Task<Producto> SeedProductoAsync(
        string codigo = "COD-001",
        string nombre = "Producto Test",
        bool activo = true,
        int stockCantidad = 10,
        int stockMinimo = 2)
    {
        var rubroId = await SeedRubroAsync();
        var producto = new Producto
        {
            Nombre = nombre,
            Codigo = codigo,
            RubroId = rubroId,
            PrecioVenta = 100,
            Costo = 60,
            StockMinimo = stockMinimo,
            Activo = activo
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = stockCantidad });
        await _context.SaveChangesAsync();

        return producto;
    }

    // ── RF-013: alta ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreaProductoConStockInicialEnCero()
    {
        var rubroId = await SeedRubroAsync();
        var dto = new ProductoCreateDto
        {
            Nombre = "Alfajor",
            Codigo = "ALF-100",
            RubroId = rubroId,
            PrecioVenta = 150,
            Costo = 90,
            StockMinimo = 5
        };

        var resultado = await _service.CreateAsync(dto);

        resultado.Id.Should().BeGreaterThan(0);
        resultado.Codigo.Should().Be("ALF-100");
        resultado.StockActual.Should().Be(0, "el stock arranca en 0 (RF-021)");

        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductoId == resultado.Id);
        stock.Should().NotBeNull("RF-013: el producto debe nacer con su registro de stock");
        stock!.Cantidad.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_CodigoDuplicado_LanzaBusinessRule()
    {
        await SeedProductoAsync(codigo: "DU-1");
        var dto = new ProductoCreateDto { Nombre = "Otro", Codigo = "DU-1", RubroId = 1, PrecioVenta = 1, Costo = 1 };

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>(
            "RF-077: no deben existir dos productos con el mismo código (índice único)");
    }

    [Fact]
    public async Task CreateAsync_RubroInexistente_LanzaEntityNotFound()
    {
        var dto = new ProductoCreateDto { Nombre = "X", Codigo = "X-1", RubroId = 9999, PrecioVenta = 1, Costo = 1 };

        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── RF-014: asociación con rubros ───────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_DevuelveElRubroAsociado()
    {
        var producto = await SeedProductoAsync();

        var resultado = await _service.GetByIdAsync(producto.Id);

        resultado.Should().NotBeNull();
        resultado!.RubroNombre.Should().Be("Rubro Test", "RF-014: el producto identifica su rubro");
    }

    // ── RF-016: búsqueda y filtros ──────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_FiltraPorNombre()
    {
        await SeedProductoAsync(codigo: "A-1", nombre: "Coca Cola");
        await SeedProductoAsync(codigo: "A-2", nombre: "Papas Fritas");

        var resultados = (await _service.SearchAsync("Coca", null, null, null, null)).ToList();

        resultados.Should().ContainSingle();
        resultados[0].Nombre.Should().Be("Coca Cola");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorCodigo()
    {
        await SeedProductoAsync(codigo: "BEB-01");
        await SeedProductoAsync(codigo: "GOL-99");

        var resultados = (await _service.SearchAsync(null, "BEB", null, null, null)).ToList();

        resultados.Should().ContainSingle(p => p.Codigo == "BEB-01");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorRubro()
    {
        var rubroA = await SeedRubroAsync("Rubro A");
        var rubroB = await SeedRubroAsync("Rubro B");
        var p1 = await SeedProductoAsync(codigo: "R-A");
        p1.RubroId = rubroA;
        var p2 = await SeedProductoAsync(codigo: "R-B");
        p2.RubroId = rubroB;
        await _context.SaveChangesAsync();

        var resultados = (await _service.SearchAsync(null, null, rubroA, null, null)).ToList();

        resultados.Should().ContainSingle(p => p.Codigo == "R-A");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorEstado()
    {
        await SeedProductoAsync(codigo: "ACT-1", activo: true);
        await SeedProductoAsync(codigo: "BAJ-1", activo: false);

        // Por defecto solo activos (RF-015).
        var porDefecto = (await _service.SearchAsync(null, null, null, null, null)).ToList();
        porDefecto.Should().ContainSingle(p => p.Codigo == "ACT-1");

        // Estado explícito: dados de baja (RF-016 permite filtrar por estado).
        var dadosDeBaja = (await _service.SearchAsync(null, null, null, activo: false, null)).ToList();
        dadosDeBaja.Should().ContainSingle(p => p.Codigo == "BAJ-1");
    }

    [Fact]
    public async Task SearchAsync_FiltraPorDisponibilidadDeStock()
    {
        await SeedProductoAsync(codigo: "CON-1", stockCantidad: 5);
        await SeedProductoAsync(codigo: "SIN-1", stockCantidad: 0);

        var conStock = (await _service.SearchAsync(null, null, null, null, conStock: true)).ToList();
        conStock.Should().ContainSingle(p => p.Codigo == "CON-1");

        var sinStock = (await _service.SearchAsync(null, null, null, null, conStock: false)).ToList();
        sinStock.Should().ContainSingle(p => p.Codigo == "SIN-1");
    }

    // ── RF-017: modificación ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ModificaCamposYPrecios()
    {
        var producto = await SeedProductoAsync();

        await _service.UpdateAsync(producto.Id, new ProductoUpdateDto
        {
            Nombre = "Actualizado",
            Codigo = producto.Codigo,
            RubroId = producto.RubroId,
            PrecioVenta = 250,
            Costo = 150,
            StockMinimo = 8
        });

        var enDb = await _context.Productos.FindAsync(producto.Id);
        enDb!.Nombre.Should().Be("Actualizado");
        enDb.PrecioVenta.Should().Be(250);
        enDb.Costo.Should().Be(150);
        enDb.StockMinimo.Should().Be(8);
    }

    [Fact]
    public async Task UpdateAsync_CodigoDuplicadoOTroProducto_LanzaBusinessRule()
    {
        await SeedProductoAsync(codigo: "UNO");
        var otro = await SeedProductoAsync(codigo: "DOS");

        var act = () => _service.UpdateAsync(otro.Id, new ProductoUpdateDto
        {
            Nombre = "X",
            Codigo = "UNO",
            RubroId = otro.RubroId,
            PrecioVenta = 1,
            Costo = 1
        });

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    // ── RF-018: baja lógica ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_DeshabilitaSinEliminarFisicamente()
    {
        var producto = await SeedProductoAsync();

        await _service.DeleteAsync(producto.Id);

        var enDb = await _context.Productos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == producto.Id);
        enDb.Should().NotBeNull("RF-073: baja lógica, nunca eliminación física");
        enDb!.Activo.Should().BeFalse();
        enDb.Nombre.Should().Be("Producto Test", "RF-074: conservar información histórica");
    }

    // ── RF-019: precios y costos ────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistePreciosYCostos()
    {
        var rubroId = await SeedRubroAsync();

        var resultado = await _service.CreateAsync(new ProductoCreateDto
        {
            Nombre = "Pilcha",
            Codigo = "PIL-1",
            RubroId = rubroId,
            PrecioVenta = 999.99m,
            Costo = 450.50m,
            StockMinimo = 3
        });

        resultado.PrecioVenta.Should().Be(999.99m);
        resultado.Costo.Should().Be(450.50m);
    }

    // ── RF-020: imágenes (carga, asociación, reemplazo, eliminación) ────────

    private static Stream FakeImage() => new MemoryStream(new byte[] { 1, 2, 3 });

    [Fact]
    public async Task UploadImageAsync_AsociaLaUrlAlProducto()
    {
        var producto = await SeedProductoAsync();

        var resultado = await _service.UploadImageAsync(
            producto.Id, FakeImage(), "foto.png", "image/png");

        resultado.ImagenUrl.Should().NotBeNullOrEmpty();
        resultado.ImagenUrl.Should().StartWith("https://storage.googleapis.com/");
        _storage.UploadedUrls.Should().ContainSingle();

        var enDb = await _context.Productos.FindAsync(producto.Id);
        enDb!.ImagenUrl.Should().Be(resultado.ImagenUrl, "RF-020: la URL se asocia en la BD (RNF-037)");
    }

    [Fact]
    public async Task UploadImageAsync_ReemplazaLaAnterior_EliminandoLaVieja()
    {
        var producto = await SeedProductoAsync();

        var primera = await _service.UploadImageAsync(producto.Id, FakeImage(), "a.png", "image/png");
        var segunda = await _service.UploadImageAsync(producto.Id, FakeImage(), "b.png", "image/png");

        segunda.ImagenUrl.Should().NotBe(primera.ImagenUrl);
        _storage.DeletedUrls.Should().ContainSingle()
            .Which.Should().Be(primera.ImagenUrl, "RF-020: reemplazo elimina la imagen anterior");
    }

    [Fact]
    public async Task RemoveImageAsync_DesasociaYEliminaDelStorage()
    {
        var producto = await SeedProductoAsync();
        var subida = await _service.UploadImageAsync(producto.Id, FakeImage(), "a.png", "image/png");

        var resultado = await _service.RemoveImageAsync(producto.Id);

        resultado.ImagenUrl.Should().BeNull();
        _storage.DeletedUrls.Should().ContainSingle().Which.Should().Be(subida.ImagenUrl);
    }

    [Fact]
    public async Task RemoveImageAsync_SinImagen_LanzaBusinessRule()
    {
        var producto = await SeedProductoAsync();

        var act = () => _service.RemoveImageAsync(producto.Id);
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task UploadImageAsync_FormatoNoPermitido_LanzaBusinessRule()
    {
        var producto = await SeedProductoAsync();

        var act = () => _service.UploadImageAsync(
            producto.Id, FakeImage(), "doc.pdf", "application/pdf");

        await act.Should().ThrowAsync<BusinessRuleException>(
            "solo se permiten imágenes JPEG/PNG/WebP/GIF (validación de RF-020)");
        _storage.UploadedUrls.Should().BeEmpty("el storage no debe recibir archivos inválidos");
    }
}
