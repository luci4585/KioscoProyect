using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<UsuarioDto?> GetByFirebaseUidAsync(string firebaseUid);
    Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);
}
