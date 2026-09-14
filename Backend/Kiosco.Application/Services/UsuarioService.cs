using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IRolRepository _rolRepository;

    public UsuarioService(IUsuarioRepository repository, IRolRepository rolRepository)
    {
        _repository = repository;
        _rolRepository = rolRepository;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();
        return usuarios.Select(u => u.ToDto());
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        return usuario?.ToDto();
    }

    public async Task<UsuarioDto?> GetByFirebaseUidAsync(string firebaseUid)
    {
        var usuario = await _repository.GetByFirebaseUidAsync(firebaseUid);
        return usuario?.ToDto();
    }

    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto)
    {
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new BusinessRuleException($"Ya existe un usuario con el email '{dto.Email}'.");

        if (!await _rolRepository.ExistsAsync(dto.RolId))
            throw new EntityNotFoundException(nameof(Rol), dto.RolId);

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            FirebaseUid = dto.FirebaseUid,
            RolId = dto.RolId
        };

        var created = await _repository.CreateAsync(usuario);
        return created.ToDto();
    }
}
