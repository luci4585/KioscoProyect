using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IIngresoMercaderiaRepository : IGenericRepository<IngresoMercaderia>
{
    Task<IngresoMercaderia?> GetWithDetailsAsync(int id);
    Task<IEnumerable<IngresoMercaderia>> GetByProveedorAsync(int proveedorId);
    Task<IEnumerable<IngresoMercaderia>> GetByRangoFechasAsync(DateTime desde, DateTime hasta);
}
