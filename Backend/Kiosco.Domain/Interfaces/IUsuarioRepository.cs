using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    /// <summary>
    /// Busca un usuario por su Firebase UID <b>sin filtrar por estado</b>.
    /// El filtro lo decide el llamador (necesario para distinguir "no existe"
    /// de "deshabilitado" en RF-005). El índice de FirebaseUid es único.
    /// </summary>
    Task<Usuario?> GetByFirebaseUidAsync(string firebaseUid);

    /// <summary>
    /// Busca un usuario por email <b>sin filtrar por estado</b>: el índice único
    /// de Email aplica a todos los registros, incluidos los dados de baja (RF-076).
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email);

    /// <summary>
    /// Busca un usuario por nombre de usuario <b>sin filtrar por estado</b>:
    /// el índice único de NombreUsuario aplica a todos los registros (RF-076).
    /// </summary>
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);

    /// <summary>
    /// Devuelve el usuario con su Rol y los permisos del rol (RolPermisos.Permiso)
    /// cargados, sin filtrar por estado. El llamador valida el estado (RF-005/RF-007).
    /// </summary>
    Task<Usuario?> GetWithPermissionsAsync(string firebaseUid);
}
