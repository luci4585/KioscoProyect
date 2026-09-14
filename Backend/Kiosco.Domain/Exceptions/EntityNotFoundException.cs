namespace Kiosco.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, int id)
        : base($"La entidad '{entityName}' con ID {id} no fue encontrada.")
    {
    }

    public EntityNotFoundException(string entityName)
        : base($"No se encontró ninguna entidad de tipo '{entityName}'.")
    {
    }
}
