using System.Reflection;
using FluentAssertions;
using Kiosco.API.Authorization;
using Kiosco.API.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Kiosco.Tests.ControllerTests;

/// <summary>
/// Test de la Etapa 3 (RoadMap §6 — Testing):
/// 6. Endpoints protegidos
/// Verifica por reflexión que todos los controllers de la API exigen
/// autenticación (RNF-026) y que las operaciones de escritura exigen
/// los permisos granulares correspondientes (RNF-034, RF-007).
/// </summary>
public class EndpointProtectionTests
{
    private static IEnumerable<Type> GetControllers()
    {
        return typeof(AuthController).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller"));
    }

    private static MethodInfo? GetAction(string controller, string action)
    {
        var type = GetControllers().Single(c => c.Name == controller);
        return type.GetMethods()
            .FirstOrDefault(m => m.Name == action && m.GetParameters().Length >= 0);
    }

    [Fact]
    public void TodosLosControllers_ExigenAutenticacion()
    {
        var controllers = GetControllers().ToList();

        controllers.Should().NotBeEmpty("debe haber al menos un controller en la API");

        foreach (var controller in controllers)
        {
            var hasAuthorize = controller
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            hasAuthorize.Should().BeTrue(
                $"el controller {controller.Name} debe estar protegido con [Authorize] (RNF-026)");
        }
    }

    [Fact]
    public void Rubros_Escrituras_RequierenPermisoRubrosGestionar()
    {
        GetAction(nameof(RubrosController), nameof(RubrosController.CreateRubro))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("rubros.gestionar");

        GetAction(nameof(RubrosController), nameof(RubrosController.UpdateRubro))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("rubros.gestionar");

        GetAction(nameof(RubrosController), nameof(RubrosController.DeleteRubro))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("rubros.gestionar");
    }

    [Fact]
    public void Rubros_Consultas_SoloExigenAutenticacion()
    {
        GetAction(nameof(RubrosController), nameof(RubrosController.GetRubros))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull("la consulta de rubros es accesible a todo usuario interno autenticado (RF-010)");

        GetAction(nameof(RubrosController), nameof(RubrosController.GetRubro))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();
    }

    [Fact]
    public void Usuarios_Endpoints_RequierenPermisosDeGestion()
    {
        // Lectura, modificación y baja → usuarios.gestionar (RF-004, RF-005, RNF-034).
        GetAction(nameof(UsuariosController), nameof(UsuariosController.GetUsuarios))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("usuarios.gestionar");

        GetAction(nameof(UsuariosController), nameof(UsuariosController.GetUsuario))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("usuarios.gestionar");

        GetAction(nameof(UsuariosController), nameof(UsuariosController.UpdateUsuario))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("usuarios.gestionar");

        GetAction(nameof(UsuariosController), nameof(UsuariosController.DeleteUsuario))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("usuarios.gestionar");

        // Asignación de rol → roles.gestionar, independiente de usuarios.gestionar (RF-006, RF-008).
        GetAction(nameof(UsuariosController), nameof(UsuariosController.UpdateRolUsuario))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("roles.gestionar");
    }

    [Fact]
    public void Productos_Endpoints_RequierenPermisosGranulares()
    {
        // RF-016 consulta → productos.consultar.
        GetAction(nameof(ProductosController), nameof(ProductosController.GetProductos))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.consultar");

        GetAction(nameof(ProductosController), nameof(ProductosController.Search))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.consultar");

        // RF-013 alta, RF-017 modificación, RF-018 baja (RNF-034).
        GetAction(nameof(ProductosController), nameof(ProductosController.CreateProducto))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.crear");

        GetAction(nameof(ProductosController), nameof(ProductosController.UpdateProducto))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.editar");

        GetAction(nameof(ProductosController), nameof(ProductosController.DeleteProducto))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.eliminar");
    }

    [Fact]
    public void Clientes_Endpoints_SiguenElPatronDePermisos()
    {
        // Consultas → solo autenticación (RF-028, RF-029).
        GetAction(nameof(ClientesController), nameof(ClientesController.GetClientes))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull("la consulta de clientes es accesible a todo usuario interno autenticado");

        GetAction(nameof(ClientesController), nameof(ClientesController.Search))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        GetAction(nameof(ClientesController), nameof(ClientesController.GetCliente))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        // Escritura → clientes.gestionar (RF-027, RF-030, RF-031, RNF-034).
        GetAction(nameof(ClientesController), nameof(ClientesController.CreateCliente))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("clientes.gestionar");

        GetAction(nameof(ClientesController), nameof(ClientesController.UpdateCliente))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("clientes.gestionar");

        GetAction(nameof(ClientesController), nameof(ClientesController.DeleteCliente))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("clientes.gestionar");
    }

    [Fact]
    public void Proveedores_Endpoints_SiguenElPatronDePermisos()
    {
        // Consultas → solo autenticación (RF-033).
        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.GetProveedores))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull("la consulta de proveedores es accesible a todo usuario interno autenticado");

        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.Search))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.GetProveedor))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        // Escritura → proveedores.gestionar (RF-032, RF-034, RF-035, RNF-034).
        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.CreateProveedor))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("proveedores.gestionar");

        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.UpdateProveedor))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("proveedores.gestionar");

        GetAction(nameof(ProveedoresController), nameof(ProveedoresController.DeleteProveedor))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("proveedores.gestionar");
    }

    [Fact]
    public void Stock_Endpoints_SiguenElPatronDePermisos()
    {
        // Consultas → solo autenticación (RF-021, RF-026).
        GetAction(nameof(StockController), nameof(StockController.GetStock))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull("la consulta de stock es accesible a todo usuario interno autenticado");

        GetAction(nameof(StockController), nameof(StockController.GetAlertas))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        GetAction(nameof(StockController), nameof(StockController.GetStockByProducto))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        // Movimientos → stock.gestionar (RF-022, RF-023, RNF-034).
        GetAction(nameof(StockController), nameof(StockController.Ingresar))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("stock.gestionar");

        GetAction(nameof(StockController), nameof(StockController.Egresar))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("stock.gestionar");
    }

    [Fact]
    public void IngresosMercaderia_Endpoints_SiguenElPatronDePermisos()
    {
        // Consultas → solo autenticación.
        GetAction(nameof(IngresosMercaderiaController), nameof(IngresosMercaderiaController.GetIngresos))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull("la consulta de ingresos es accesible a todo usuario interno autenticado");

        GetAction(nameof(IngresosMercaderiaController), nameof(IngresosMercaderiaController.GetIngreso))!
            .GetCustomAttribute<RequirePermissionAttribute>()
            .Should().BeNull();

        // Escritura → stock.gestionar (RF-038: el ingreso es un movimiento de stock).
        GetAction(nameof(IngresosMercaderiaController), nameof(IngresosMercaderiaController.CreateIngreso))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("stock.gestionar");
    }

    [Fact]
    public void Ventas_Endpoints_RequierenPermisosDeVentas()
    {
        // Consulta → ventas.consultar (RF-048, RF-049, RNF-034).
        GetAction(nameof(VentasController), nameof(VentasController.GetVentas))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("ventas.consultar");

        GetAction(nameof(VentasController), nameof(VentasController.GetVenta))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("ventas.consultar");

        // Registro → ventas.crear (RF-040).
        GetAction(nameof(VentasController), nameof(VentasController.CreateVenta))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("ventas.crear");

        // Anulación → ventas.anular (RF-050).
        GetAction(nameof(VentasController), nameof(VentasController.AnularVenta))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("ventas.anular");
    }

    [Fact]
    public void AuthMe_NoRequierePermiso_PeroSiAutenticacion()
    {
        // /api/auth/me es el punto de entrada tras el login (RF-001): exige
        // autenticación pero ningún permiso en particular, porque el usuario
        // recién autenticado aún no tiene contexto de permisos cargado.
        var controllerType = GetControllers().Single(c => c.Name == nameof(AuthController));
        var action = GetAction(nameof(AuthController), nameof(AuthController.GetMe))!;

        controllerType.GetCustomAttribute<AuthorizeAttribute>(inherit: true).Should().NotBeNull(
            "el controller Auth debe exigir autenticación (RNF-026)");
        action.GetCustomAttribute<RequirePermissionAttribute>().Should().BeNull();
    }

    [Fact]
    public void RequirePermission_GeneraPoliticaConElCodigoIndicado()
    {
        var attribute = new RequirePermissionAttribute("rubros.gestionar");

        attribute.Codigo.Should().Be("rubros.gestionar");
        attribute.Policy.Should().Be("Permission:rubros.gestionar");
    }
}
