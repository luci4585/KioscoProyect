using Kiosco.Application.DTOs;
using Kiosco.Domain.Entities;

namespace Kiosco.Application.Mappings;

public static class EntityMapper
{
    public static RubroDto ToDto(this Rubro entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        Activo = entity.Activo
    };

    public static ProductoDto ToDto(this Producto entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        Codigo = entity.Codigo,
        RubroId = entity.RubroId,
        RubroNombre = entity.Rubro?.Nombre ?? string.Empty,
        PrecioVenta = entity.PrecioVenta,
        Costo = entity.Costo,
        StockActual = entity.Stock?.Cantidad ?? 0,
        StockMinimo = entity.StockMinimo,
        ImagenUrl = entity.ImagenUrl,
        Activo = entity.Activo
    };

    public static ClienteDto ToDto(this Cliente entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Apellido = entity.Apellido,
        Documento = entity.Documento,
        Telefono = entity.Telefono,
        Email = entity.Email,
        Direccion = entity.Direccion,
        Activo = entity.Activo
    };

    public static ProveedorDto ToDto(this Proveedor entity) => new()
    {
        Id = entity.Id,
        RazonSocial = entity.RazonSocial,
        Cuit = entity.Cuit,
        Telefono = entity.Telefono,
        Email = entity.Email,
        Direccion = entity.Direccion,
        Activo = entity.Activo
    };

    public static StockDto ToDto(this Stock entity) => new()
    {
        Id = entity.Id,
        ProductoId = entity.ProductoId,
        ProductoNombre = entity.Producto?.Nombre ?? string.Empty,
        ProductoCodigo = entity.Producto?.Codigo ?? string.Empty,
        Cantidad = entity.Cantidad,
        StockMinimo = entity.Producto?.StockMinimo ?? 0,
        StockBajo = entity.Cantidad <= (entity.Producto?.StockMinimo ?? 0)
    };

    public static VentaDto ToDto(this Venta entity) => new()
    {
        Id = entity.Id,
        Fecha = entity.Fecha,
        UsuarioId = entity.UsuarioId,
        UsuarioNombre = entity.Usuario != null ? $"{entity.Usuario.Nombre} {entity.Usuario.Apellido}" : string.Empty,
        ClienteId = entity.ClienteId,
        ClienteNombre = entity.Cliente != null ? $"{entity.Cliente.Nombre} {entity.Cliente.Apellido}" : null,
        Total = entity.Total,
        MetodoPago = entity.MetodoPago,
        Anulada = entity.Anulada,
        Detalles = entity.DetallesVenta?.Select(d => d.ToDto()).ToList() ?? new List<DetalleVentaDto>()
    };

    public static DetalleVentaDto ToDto(this DetalleVenta entity) => new()
    {
        Id = entity.Id,
        ProductoId = entity.ProductoId,
        ProductoNombre = entity.Producto?.Nombre ?? string.Empty,
        Cantidad = entity.Cantidad,
        PrecioUnitario = entity.PrecioUnitario,
        Subtotal = entity.Subtotal
    };

    public static CajaDto ToDto(this Caja entity) => new()
    {
        Id = entity.Id,
        FechaApertura = entity.FechaApertura,
        FechaCierre = entity.FechaCierre,
        MontoInicial = entity.MontoInicial,
        MontoFinal = entity.MontoFinal,
        Diferencia = entity.Diferencia,
        Abierta = entity.Abierta,
        UsuarioAperturaNombre = entity.UsuarioApertura != null ? $"{entity.UsuarioApertura.Nombre} {entity.UsuarioApertura.Apellido}" : string.Empty,
        UsuarioCierreNombre = entity.UsuarioCierre != null ? $"{entity.UsuarioCierre.Nombre} {entity.UsuarioCierre.Apellido}" : null,
        TotalIngresos = entity.Movimientos?.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto) ?? 0,
        TotalEgresos = entity.Movimientos?.Where(m => m.TipoMovimiento == "Egreso").Sum(m => m.Monto) ?? 0,
        SaldoCalculado = (entity.MontoInicial
            + (entity.Movimientos?.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto) ?? 0)
            - (entity.Movimientos?.Where(m => m.TipoMovimiento == "Egreso").Sum(m => m.Monto) ?? 0))
    };

    public static MovimientoCajaDto ToDto(this MovimientoCaja entity) => new()
    {
        Id = entity.Id,
        Fecha = entity.Fecha,
        TipoMovimiento = entity.TipoMovimiento,
        Monto = entity.Monto,
        Descripcion = entity.Descripcion,
        UsuarioNombre = entity.Usuario != null ? $"{entity.Usuario.Nombre} {entity.Usuario.Apellido}" : string.Empty
    };

    public static IngresoMercaderiaDto ToDto(this IngresoMercaderia entity) => new()
    {
        Id = entity.Id,
        Fecha = entity.Fecha,
        ProveedorId = entity.ProveedorId,
        ProveedorNombre = entity.Proveedor?.RazonSocial ?? string.Empty,
        Observaciones = entity.Observaciones,
        Detalles = entity.Detalles?.Select(d => d.ToDto()).ToList() ?? new List<DetalleIngresoMercaderiaDto>()
    };

    public static DetalleIngresoMercaderiaDto ToDto(this DetalleIngresoMercaderia entity) => new()
    {
        Id = entity.Id,
        ProductoId = entity.ProductoId,
        ProductoNombre = entity.Producto?.Nombre ?? string.Empty,
        Cantidad = entity.Cantidad,
        CostoUnitario = entity.CostoUnitario
    };

    public static UsuarioDto ToDto(this Usuario entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Apellido = entity.Apellido,
        NombreUsuario = entity.NombreUsuario,
        Email = entity.Email,
        FirebaseUid = entity.FirebaseUid,
        RolId = entity.RolId,
        RolNombre = entity.Rol?.Nombre ?? string.Empty,
        Activo = entity.Activo
    };
}
