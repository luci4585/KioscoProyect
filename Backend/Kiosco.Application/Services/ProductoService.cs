using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class ProductoService : IProductoService
{
    /// <summary>Tamaño máximo de imagen: 5 MB (RNF-021 validación de entrada).</summary>
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private readonly IProductoRepository _productoRepository;
    private readonly IRubroRepository _rubroRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IStorageService _storageService;

    public ProductoService(
        IProductoRepository productoRepository,
        IRubroRepository rubroRepository,
        IStockRepository stockRepository,
        IStorageService storageService)
    {
        _productoRepository = productoRepository;
        _rubroRepository = rubroRepository;
        _stockRepository = stockRepository;
        _storageService = storageService;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        var productos = await _productoRepository.GetAllAsync();
        return productos.Select(p => p.ToDto());
    }

    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        return producto?.ToDto();
    }

    public async Task<IEnumerable<ProductoDto>> SearchAsync(
        string? nombre,
        string? codigo,
        int? rubroId,
        bool? activo,
        bool? conStock)
    {
        var productos = await _productoRepository.SearchAsync(nombre, codigo, rubroId, activo, conStock);
        return productos.Select(p => p.ToDto());
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateDto dto)
    {
        // RF-076/RF-077: unicidad de código (índice único en BD).
        var codigoExistente = await _productoRepository.GetByCodigoAsync(dto.Codigo);
        if (codigoExistente != null)
            throw new BusinessRuleException($"Ya existe un producto con el código '{dto.Codigo}'.");

        if (!await _rubroRepository.ExistsAsync(dto.RubroId))
            throw new EntityNotFoundException(nameof(Rubro), dto.RubroId);

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Codigo = dto.Codigo,
            RubroId = dto.RubroId,
            PrecioVenta = dto.PrecioVenta,
            Costo = dto.Costo,
            StockMinimo = dto.StockMinimo
        };

        var created = await _productoRepository.CreateAsync(producto);

        var stock = new Stock
        {
            ProductoId = created.Id,
            Cantidad = 0
        };
        await _stockRepository.CreateAsync(stock);

        return created.ToDto();
    }

    public async Task UpdateAsync(int id, ProductoUpdateDto dto)
    {
        var producto = await _productoRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Producto), id);

        var codigoExistente = await _productoRepository.GetByCodigoAsync(dto.Codigo);
        if (codigoExistente != null && codigoExistente.Id != id)
            throw new BusinessRuleException($"Ya existe un producto con el código '{dto.Codigo}'.");

        if (!await _rubroRepository.ExistsAsync(dto.RubroId))
            throw new EntityNotFoundException(nameof(Rubro), dto.RubroId);

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Codigo = dto.Codigo;
        producto.RubroId = dto.RubroId;
        producto.PrecioVenta = dto.PrecioVenta;
        producto.Costo = dto.Costo;
        producto.StockMinimo = dto.StockMinimo;

        await _productoRepository.UpdateAsync(producto);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _productoRepository.ExistsAsync(id))
            throw new EntityNotFoundException(nameof(Producto), id);

        await _productoRepository.DeleteAsync(id);
    }

    public async Task<ProductoDto> UploadImageAsync(
        int id,
        Stream content,
        string fileName,
        string contentType)
    {
        var producto = await _productoRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Producto), id);

        ValidateImage(content, contentType);

        // Carga primero; solo si la carga tiene éxito se reemplaza la referencia
        // (RF-020: carga → asociación → reemplazo).
        var url = await _storageService.UploadImageAsync(content, fileName, contentType);

        var imagenAnterior = producto.ImagenUrl;
        producto.ImagenUrl = url;
        await _productoRepository.UpdateAsync(producto);

        // Reemplazo: elimina la anterior después de asociar la nueva.
        if (!string.IsNullOrEmpty(imagenAnterior) && imagenAnterior != url)
            await _storageService.DeleteImageAsync(imagenAnterior);

        return producto.ToDto();
    }

    public async Task<ProductoDto> RemoveImageAsync(int id)
    {
        var producto = await _productoRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Producto), id);

        var imagen = producto.ImagenUrl;
        if (string.IsNullOrEmpty(imagen))
            throw new BusinessRuleException("El producto no tiene imagen asociada.");

        producto.ImagenUrl = null;
        await _productoRepository.UpdateAsync(producto);

        // Eliminación del storage después de desasociar: el estado de la BD
        // es la fuente de verdad; si falla el storage la URL queda huérfana
        // pero inaccesible desde el sistema.
        await _storageService.DeleteImageAsync(imagen);

        return producto.ToDto();
    }

    private static void ValidateImage(Stream content, string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedImageTypes.Contains(contentType))
            throw new BusinessRuleException(
                "Formato de imagen no soportado. Se permiten JPEG, PNG, WebP o GIF.");

        if (content.CanSeek && content.Length > MaxImageBytes)
            throw new BusinessRuleException("La imagen no puede superar los 5 MB.");
    }
}
