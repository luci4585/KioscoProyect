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

        // RF-020 imágenes: requiere permiso de edición del producto.
        GetAction(nameof(ProductosController), nameof(ProductosController.UploadImagen))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.editar");

        GetAction(nameof(ProductosController), nameof(ProductosController.DeleteImagen))!
            .GetCustomAttribute<RequirePermissionAttribute>()?.Codigo
            .Should().Be("productos.editar");
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
