using ClosedXML.Excel;
using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;

namespace Kiosco.Infrastructure.Excel;

/// <summary>
/// Generación de Excel en el backend con **ClosedXML** (RNF-057, RNF-058) —
/// Etapa 11. Cada método construye una hoja con título, encabezados y filas.
/// </summary>
public class ExcelExportAdapter : IExcelExportService
{
    private static byte[] Build(string sheetName, string titulo, string[] headers, IEnumerable<object[]> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Título (fila 1)
        var tituloCell = worksheet.Cell(1, 1);
        tituloCell.Value = titulo;
        tituloCell.Style.Font.Bold = true;
        tituloCell.Style.Font.FontSize = 14;

        // Encabezados (fila 2)
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(2, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
        }

        // Datos (desde la fila 3)
        var rowNumber = 3;
        foreach (var row in rows)
        {
            for (var i = 0; i < row.Length; i++)
                worksheet.Cell(rowNumber, i + 1).Value = ToCellValue(row[i]);
            rowNumber++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static XLCellValue ToCellValue(object? value) => value switch
    {
        null => default(XLCellValue),
        string s => s,
        decimal d => d,
        int i => i,
        long l => l,
        bool b => b,
        DateTime dt => dt,
        _ => value.ToString() ?? string.Empty
    };

    private static string Money(decimal value) => value.ToString("0.##");

    public byte[] ExportVentas(string titulo, IEnumerable<VentaDto> ventas)
    {
        var rows = ventas.Select(v => new object[]
        {
            v.Id,
            v.Fecha,
            v.ClienteNombre ?? "Sin cliente",
            v.UsuarioNombre,
            v.MetodoPago,
            Money(v.Total),
            v.Anulada ? "Sí" : "No"
        });
        return Build("Ventas", titulo,
            new[] { "Id", "Fecha", "Cliente", "Vendedor", "Método de pago", "Total", "Anulada" },
            rows);
    }

    public byte[] ExportProductos(string titulo, IEnumerable<ProductoDto> productos)
    {
        var rows = productos.Select(p => new object[]
        {
            p.Codigo,
            p.Nombre,
            p.RubroNombre,
            Money(p.PrecioVenta),
            Money(p.Costo),
            p.StockActual,
            p.StockMinimo,
            p.Activo ? "Activo" : "Baja"
        });
        return Build("Productos", titulo,
            new[] { "Código", "Nombre", "Rubro", "Precio venta", "Costo", "Stock", "Mínimo", "Estado" },
            rows);
    }

    public byte[] ExportStock(string titulo, IEnumerable<StockDto> stock)
    {
        var rows = stock.Select(s => new object[]
        {
            s.ProductoCodigo,
            s.ProductoNombre,
            s.Cantidad,
            s.StockMinimo,
            s.StockBajo ? "Sí" : "No"
        });
        return Build("Stock", titulo,
            new[] { "Código", "Producto", "Cantidad", "Mínimo", "Stock bajo" },
            rows);
    }

    public byte[] ExportMovimientosCaja(string titulo, IEnumerable<MovimientoCajaDto> movimientos)
    {
        var rows = movimientos.Select(m => new object[]
        {
            m.Fecha,
            m.TipoMovimiento,
            Money(m.Monto),
            m.Descripcion ?? "",
            m.UsuarioNombre
        });
        return Build("Movimientos de caja", titulo,
            new[] { "Fecha", "Tipo", "Monto", "Descripción", "Responsable" },
            rows);
    }

    public byte[] ExportIngresosMercaderia(string titulo, IEnumerable<IngresoMercaderiaDto> ingresos)
    {
        // Filas de detalle: cada producto de cada ingreso (RF-039 costos).
        var rows = ingresos.SelectMany(i => i.Detalles.Select(d => new object[]
        {
            i.Id,
            i.Fecha,
            i.ProveedorNombre,
            d.ProductoNombre,
            d.Cantidad,
            Money(d.CostoUnitario),
            Money(d.Cantidad * d.CostoUnitario),
            i.Observaciones ?? ""
        }));
        return Build("Ingresos de mercadería", titulo,
            new[] { "Ingreso", "Fecha", "Proveedor", "Producto", "Cantidad", "Costo unitario", "Subtotal", "Observaciones" },
            rows);
    }

    public byte[] ExportResumenDiarios(string titulo, IEnumerable<ResumenDiarioDto> resumenes)
    {
        var rows = resumenes.Select(r => new object[]
        {
            r.Fecha,
            Money(r.MontoInicialCaja),
            Money(r.TotalIngresos),
            Money(r.TotalEgresos),
            Money(r.MontoFinalCaja),
            r.CantidadVentas,
            Money(r.TotalVentas),
            r.ProductosVendidos,
            Money(r.CostoVendido),
            Money(r.Ganancia),
            r.UsuarioApertura ?? "",
            r.UsuarioCierre ?? ""
        });
        return Build("Informes diarios", titulo,
            new[] { "Fecha", "Monto inicial", "Ingresos", "Egresos", "Monto final", "Cant. ventas",
                    "Total vendido", "Productos vendidos", "Costo vendido", "Ganancia",
                    "Apertura", "Cierre" },
            rows);
    }

    public byte[] ExportGanancias(string titulo, IEnumerable<InformeGananciaDto> ganancias)
    {
        var rows = ganancias.Select(g => new object[]
        {
            g.Desde,
            g.Hasta,
            Money(g.TotalVendido),
            Money(g.CostoVendido),
            Money(g.Ganancia)
        });
        return Build("Ganancias", titulo,
            new[] { "Desde", "Hasta", "Total vendido", "Costo vendido", "Ganancia" },
            rows);
    }
}
