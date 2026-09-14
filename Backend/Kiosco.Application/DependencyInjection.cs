using FluentValidation;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kiosco.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRubroService, RubroService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<ICajaService, CajaService>();
        services.AddScoped<IIngresoMercaderiaService, IngresoMercaderiaService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
