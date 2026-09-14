using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class RubroService : IRubroService
{
    private readonly IRubroRepository _repository;

    public RubroService(IRubroRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RubroDto>> GetAllAsync()
    {
        var rubros = await _repository.GetAllAsync();
        return rubros.Select(r => r.ToDto());
    }

    public async Task<RubroDto?> GetByIdAsync(int id)
    {
        var rubro = await _repository.GetByIdAsync(id);
        return rubro?.ToDto();
    }

    public async Task<RubroDto> CreateAsync(RubroCreateDto dto)
    {
        var existing = await _repository.GetByNameAsync(dto.Nombre);
        if (existing != null)
            throw new BusinessRuleException($"Ya existe un rubro con el nombre '{dto.Nombre}'.");

        var rubro = new Rubro
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        var created = await _repository.CreateAsync(rubro);
        return created.ToDto();
    }

    public async Task UpdateAsync(int id, RubroUpdateDto dto)
    {
        var rubro = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Rubro), id);

        var existing = await _repository.GetByNameAsync(dto.Nombre);
        if (existing != null && existing.Id != id)
            throw new BusinessRuleException($"Ya existe un rubro con el nombre '{dto.Nombre}'.");

        rubro.Nombre = dto.Nombre;
        rubro.Descripcion = dto.Descripcion;

        await _repository.UpdateAsync(rubro);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            throw new EntityNotFoundException(nameof(Rubro), id);

        await _repository.DeleteAsync(id);
    }
}
