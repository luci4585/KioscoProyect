using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByFirebaseUidAsync(string firebaseUid);
    Task<Usuario?> GetByEmailAsync(string email);
}
