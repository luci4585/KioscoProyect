using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Kiosco.Application.DTOs;
using Kiosco.Application.Services;
using Kiosco.Domain.Entities;
using Kiosco.Infrastructure.Excel;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kiosco.Tests.IntegrationTests;

/// <summary>
/// Pruebas de integración completas — Etapa 13 (RoadMap §16; SDD §27.3).
///
/// Valora los 4 escenarios completos contra una BASE DE DATOS POSTGRESQL DE
/// TESTING real (kiosco_db_test), distinta de kiosco_db de producción. La
/// base se recrea (drop + create) en cada test y se aplica la migración
/// existente, por lo que el esquema y las semillas son idénticos a los de
/// producción. Requiere PostgreSQL local en marcha.
/// </summary>
public class EscenariosIntegracionTests : IDisposable
{
    private const string TestDatabase = "kiosco_db_test";
    private const string AdminFirebaseUid = "ADMIN_FIREBASE_UID_CAMBIAR";

    private static readonly string BaseConnectionString = ReadDefaultConnection();

    private readonly KioscoDbContext _context;

    public EscenariosIntegracionTests()
    {
        // Base dedicada de testing (SDD §27.3): recreada por test para arrancar
        // siempre con el esquema y las semillas exactos.
        RecrearBaseDeDatos();
        _context = new KioscoDbContext(new DbContextOptionsBuilder<KioscoDbContext>()
            .UseNpgsql(TestConnectionString)
            .Options);
        _context.Database.Migrate();
    }

    public void Dispose() => _context.Dispose();

    private static string TestConnectionString
    {
        get
        {
            // Pooling desactivado: el pool es estático por connection string y
            // la base se recrea (drop) en cada test; con pool apagado cada
            // test usa conexiones frescas y no hay sockets muertos entre tests.
            var builder = new NpgsqlConnectionStringBuilder(BaseConnectionString)
            {
                Database = TestDatabase,
                Pooling = false
            };
            return builder.ConnectionString;
        }
    }

    private static string ReadDefaultConnection()
    {
        // Reproduce la cadena de configuración de la API: appsettings.json y
        // luego User Secrets (que es donde está el password real de la BD).
        // Se toma subiendo desde el directorio de salida del test.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var apiDir = Path.Combine(dir.FullName, "Backend", "Kiosco.API");
            var appsettings = Path.Combine(apiDir, "appsettings.json");
            if (File.Exists(appsettings))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(appsettings));
                var conexion = doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("DefaultConnection")
                    .GetString()
                    ?? throw new InvalidOperationException("DefaultConnection vacío en appsettings.json");

                // User Secrets (password real): sobreescribe si existe. Las
                // claves pueden venir planas ("ConnectionStrings:DefaultConnection")
                // o anidadas, según cómo se generaron.
                var csproj = Path.Combine(apiDir, "Kiosco.API.csproj");
                var match = Regex.Match(
                    File.ReadAllText(csproj),
                    "<UserSecretsId>([^<]+)</UserSecretsId>");
                if (match.Success)
                {
                    var secretsPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "microsoft", "UserSecrets", match.Groups[1].Value, "secrets.json");
                    if (File.Exists(secretsPath))
                    {
                        using var secrets = JsonDocument.Parse(File.ReadAllText(secretsPath));
                        if (secrets.RootElement.TryGetProperty(
                                "ConnectionStrings:DefaultConnection", out var flat))
                        {
                            conexion = flat.GetString() ?? conexion;
                        }
                        else if (secrets.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                                 cs.TryGetProperty("DefaultConnection", out var conn))
                        {
                            conexion = conn.GetString() ?? conexion;
                        }
                    }
                }

                return conexion;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "No se encontró Backend/Kiosco.API/appsettings.json desde " + AppContext.BaseDirectory);
    }

    private static void RecrearBaseDeDatos()
    {
        var adminConnectionString = new NpgsqlConnectionStringBuilder(BaseConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;

        using var conn = new NpgsqlConnection(adminConnectionString);
        conn.Open();

        using (var terminate = new NpgsqlCommand(
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
            $"WHERE datname = '{TestDatabase}' AND pid <> pg_backend_pid();", conn))
        {
            terminate.ExecuteNonQuery();
        }

        using (var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS {TestDatabase};", conn))
        {
            drop.ExecuteNonQuery();
        }

        using (var create = new NpgsqlCommand($"CREATE DATABASE {TestDatabase};", conn))
        {
            create.ExecuteNonQuery();
        }
    }

    private async Task<int> SeedUsuarioAdminAsync()
    {
        var admin = await _context.Usuarios
            .FirstAsync(u => u.FirebaseUid == AdminFirebaseUid);
        return admin.Id;
    }

    private async Task<int> SeedProductoAsync(decimal precioVenta = 100, decimal costo = 60, string codigo = "INT-1")
    {
        var rubro = new Rubro { Nombre = "Rubro Integración", Descripcion = "Etapa 13" };
        _context.Rubros.Add(rubro);
        await _context.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = "Producto Integración",
            Codigo = codigo,
            RubroId = rubro.Id,
            PrecioVenta = precioVenta,
            Costo = costo,
            StockMinimo = 2
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductoId = producto.Id, Cantidad = 0 });
        await _context.SaveChangesAsync();

        return producto.Id;
    }

    // ── Escenario 1 — Producto (RoadMap §16) ───────────────────────────────

    [Fact]
    public async Task Escenario1_Producto_CrearRubroCrearProductoAsignarStockConsultar()
    {
        _context.Database.ProviderName.Should().Be(
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "los escenarios de la Etapa 13 deben correr sobre PostgreSQL de testing, no en memoria");

        // Crear rubro
        var rubroService = new RubroService(new RubroRepository(_context));
        var rubro = await rubroService.CreateAsync(new RubroCreateDto
        {
            Nombre = "Rubro Escenario 1",
            Descripcion = "Prueba de integración"
        });
        rubro.Id.Should().BeGreaterThan(0);

        // Crear producto
        var productoService = new ProductoService(
            new ProductoRepository(_context),
            new RubroRepository(_context),
            new StockRepository(_context));
        var producto = await productoService.CreateAsync(new ProductoCreateDto
        {
            Nombre = "Producto Escenario 1",
            Codigo = "ESC1-001",
            RubroId = rubro.Id,
            PrecioVenta = 120,
            Costo = 70,
            StockMinimo = 3
        });
        producto.Id.Should().BeGreaterThan(0);
        producto.StockActual.Should().Be(0, "el producto nace sin stock");

        // Asignar stock
        var stockService = new StockService(
            new StockRepository(_context),
            new ProductoRepository(_context));
        var stock = await stockService.IngresarAsync(producto.Id, 10);
        stock.Cantidad.Should().Be(10);

        // Consultar producto
        var consultado = await productoService.GetByIdAsync(producto.Id);
        consultado.Should().NotBeNull();
        consultado!.StockActual.Should().Be(10, "la consulta refleja el stock asignado");
        consultado.RubroNombre.Should().Be("Rubro Escenario 1");
        consultado.PrecioVenta.Should().Be(120);
    }

    // ── Escenario 2 — Mercadería (RoadMap §16) ─────────────────────────────

    [Fact]
    public async Task Escenario2_Mercaderia_ProveedorIngresoProductosStock()
    {
        var productoId = await SeedProductoAsync(codigo: "ESC2-001");

        // Crear proveedor
        var proveedorService = new ProveedorService(new ProveedorRepository(_context));
        var proveedor = await proveedorService.CreateAsync(new ProveedorCreateDto
        {
            RazonSocial = "Proveedor Escenario 2",
            Cuit = "30111223344"
        });
        proveedor.Id.Should().BeGreaterThan(0);

        // Registrar ingreso con sus productos (detalles), cantidades y costos
        var ingresoService = new IngresoMercaderiaService(
            new IngresoMercaderiaRepository(_context),
            new ProveedorRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context));
        var ingreso = await ingresoService.CreateAsync(new IngresoMercaderiaCreateDto
        {
            ProveedorId = proveedor.Id,
            Observaciones = "Ingreso de integración",
            Detalles = new List<DetalleIngresoMercaderiaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 5, CostoUnitario = 75 }
            }
        });

        // Asociación con proveedor (RF-037) y costos (RF-039)
        ingreso.ProveedorId.Should().Be(proveedor.Id);
        ingreso.ProveedorNombre.Should().Be("Proveedor Escenario 2");
        ingreso.Detalles.Should().ContainSingle();
        ingreso.Detalles[0].Cantidad.Should().Be(5);
        ingreso.Detalles[0].CostoUnitario.Should().Be(75);

        // Actualizar stock (RF-038) y verificar (RF-021)
        var stockService = new StockService(
            new StockRepository(_context),
            new ProductoRepository(_context));
        var stock = await stockService.GetByProductoAsync(productoId);
        stock.Should().NotBeNull();
        stock!.Cantidad.Should().Be(5, "0 + 5 del ingreso");
    }

    // ── Escenario 3 — Venta (RoadMap §16) ──────────────────────────────────

    [Fact]
    public async Task Escenario3_Venta_CajaClienteVentaStockMovimiento()
    {
        var usuarioId = await SeedUsuarioAdminAsync();
        var productoId = await SeedProductoAsync(precioVenta: 100, costo: 60, codigo: "ESC3-001");

        // Asignar stock disponible para vender
        var stockService = new StockService(
            new StockRepository(_context),
            new ProductoRepository(_context));
        await stockService.IngresarAsync(productoId, 10);

        var cajaService = new CajaService(
            new CajaRepository(_context),
            new MovimientoCajaRepository(_context));

        // Abrir caja
        var caja = await cajaService.AbrirAsync(new CajaAperturaDto { MontoInicial = 1000 }, usuarioId);
        caja.Abierta.Should().BeTrue();
        caja.MontoInicial.Should().Be(1000);

        // Seleccionar cliente (RF-045)
        var clienteService = new ClienteService(new ClienteRepository(_context));
        var cliente = await clienteService.CreateAsync(new ClienteCreateDto
        {
            Nombre = "Cliente",
            Apellido = "Escenario 3",
            Documento = "30555666"
        });

        // Confirmar venta con productos (RF-040 a RF-043)
        var ventaService = new VentaService(
            new VentaRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context),
            new ClienteRepository(_context),
            new CajaRepository(_context),
            new MovimientoCajaRepository(_context));
        var venta = await ventaService.CreateAsync(new VentaCreateDto
        {
            ClienteId = cliente.Id,
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 2 }
            }
        }, usuarioId);

        venta.Total.Should().Be(200, "2 × 100");
        venta.ClienteNombre.Should().Be("Cliente Escenario 3");
        venta.UsuarioNombre.Should().NotBeNullOrWhiteSpace("RF-046: vendedor registrado");

        // Actualizar stock (RF-047)
        var stock = await stockService.GetByProductoAsync(productoId);
        stock!.Cantidad.Should().Be(8, "10 − 2");

        // Registrar movimiento (RF-053: lo genera la venta) y verificar caja
        var cajaAbierta = await cajaService.GetCajaAbiertaAsync();
        cajaAbierta.Should().NotBeNull();
        cajaAbierta!.Movimientos.Should().Contain(
            m => m.Descripcion == $"Venta #{venta.Id}" && m.Monto == 200 && m.TipoMovimiento == "Ingreso");
        cajaAbierta.SaldoCalculado.Should().Be(1200, "1000 + 200 (RF-067)");
    }

    // ── Escenario 4 — Cierre (RoadMap §16) ─────────────────────────────────

    [Fact]
    public async Task Escenario4_Cierre_VentasMasIngresosMenosEgresosCierreInforme()
    {
        var usuarioId = await SeedUsuarioAdminAsync();
        var productoId = await SeedProductoAsync(precioVenta: 150, costo: 90, codigo: "ESC4-001");

        var stockService = new StockService(
            new StockRepository(_context),
            new ProductoRepository(_context));
        await stockService.IngresarAsync(productoId, 10);

        var cajaService = new CajaService(
            new CajaRepository(_context),
            new MovimientoCajaRepository(_context));

        // Abrir caja
        var caja = await cajaService.AbrirAsync(new CajaAperturaDto { MontoInicial = 1000 }, usuarioId);

        // Ingresos y egresos manuales (RF-052)
        await cajaService.RegistrarMovimientoAsync(caja.Id, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Ingreso", Monto = 300, Descripcion = "Ingreso de integración"
        }, usuarioId);
        await cajaService.RegistrarMovimientoAsync(caja.Id, new MovimientoCajaCreateDto
        {
            TipoMovimiento = "Egreso", Monto = 100, Descripcion = "Egreso de integración"
        }, usuarioId);

        // Ventas (genera su propio movimiento de ingreso — RF-053)
        var ventaService = new VentaService(
            new VentaRepository(_context),
            new ProductoRepository(_context),
            new StockRepository(_context),
            new ClienteRepository(_context),
            new CajaRepository(_context),
            new MovimientoCajaRepository(_context));
        await ventaService.CreateAsync(new VentaCreateDto
        {
            MetodoPago = "Efectivo",
            Detalles = new List<DetalleVentaCreateDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        }, usuarioId);

        // Fórmula del escenario: Ventas + Ingresos − Egresos
        //   ingresos de caja = 300 (manual) + 150 (venta) = 450
        //   egresos = 100 → saldo = 1000 + 450 − 100 = 1350
        var resumenPrevio = await cajaService.GetCajaAbiertaAsync();
        resumenPrevio!.TotalIngresos.Should().Be(450);
        resumenPrevio.TotalEgresos.Should().Be(100);
        resumenPrevio.SaldoCalculado.Should().Be(1350);

        // Cierre de caja (RF-056/RF-057): informado = calculado → diferencia 0
        var cerrada = await cajaService.CerrarAsync(
            caja.Id, new CajaCierreDto { MontoFinalInformado = 1350 }, usuarioId);
        cerrada.Abierta.Should().BeFalse();
        cerrada.MontoFinal.Should().Be(1350);
        cerrada.Diferencia.Should().Be(0);

        // Informe diario consistente con el cierre (RF-064/065/067/079)
        var informeService = new InformeService(
            new VentaRepository(_context),
            new CajaRepository(_context),
            new IngresoMercaderiaRepository(_context),
            new StockRepository(_context),
            new ProductoRepository(_context),
            new ExcelExportAdapter());
        var resumen = await informeService.GetResumenDiarioAsync(DateTime.UtcNow.Date);

        resumen.TotalVentas.Should().Be(150, "RF-064: la venta del día");
        resumen.CostoVendido.Should().Be(90, "1 × costo vigente 90 (RF-065)");
        resumen.Ganancia.Should().Be(60, "150 − 90 (RF-065)");
        resumen.TotalIngresos.Should().Be(450);
        resumen.TotalEgresos.Should().Be(100);
        resumen.MontoFinalCaja.Should().Be(1350, "RF-067 y consistencia con el cierre (RF-079)");
        resumen.CajaAbierta.Should().BeFalse();
        resumen.CantidadVentas.Should().Be(1);
        resumen.ProductosVendidos.Should().Be(1);
        resumen.UsuarioApertura.Should().NotBeNullOrWhiteSpace("RF-068");
        resumen.UsuarioCierre.Should().NotBeNullOrWhiteSpace("RF-068");
    }
}
