using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<UsuarioDto?> GetByFirebaseUidAsync(string firebaseUid);
    Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);
    Task<UsuarioDto> UpdateAsync(int id, UsuarioUpdateDto dto);
    Task<UsuarioDto> UpdateRolAsync(int id, UsuarioRolUpdateDto dto);
    Task DeleteAsync(int id);
}
