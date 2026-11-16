using Kiosco.Application.Interfaces;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Firebase.Storage;
using Kiosco.Infrastructure.Persistence.Context;
using Kiosco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kiosco.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<KioscoDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Adaptador de imágenes (RF-020, RNF-035/RNF-036): puerto en Application,
        // implementación real en Infrastructure (SDD §30.8).
        services.AddScoped<IStorageService, FirebaseStorageAdapter>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IRubroRepository, RubroRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<IIngresoMercaderiaRepository, IngresoMercaderiaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IMovimientoCajaRepository, MovimientoCajaRepository>();

        return services;
    }
}
