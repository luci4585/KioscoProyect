using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _repository;

    public ProveedorService(IProveedorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProveedorDto>> GetAllAsync()
    {
        var proveedores = await _repository.GetAllAsync();
        return proveedores.Select(p => p.ToDto());
    }

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        var proveedor = await _repository.GetByIdAsync(id);
        return proveedor?.ToDto();
    }

    public async Task<ProveedorDto> CreateAsync(ProveedorCreateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Cuit))
        {
            var existing = await _repository.GetByCuitAsync(dto.Cuit);
            if (existing != null)
                throw new BusinessRuleException($"Ya existe un proveedor con el CUIT '{dto.Cuit}'.");
        }

        var proveedor = new Proveedor
        {
            RazonSocial = dto.RazonSocial,
            Cuit = dto.Cuit,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion
        };

        var created = await _repository.CreateAsync(proveedor);
        return created.ToDto();
    }

    public async Task UpdateAsync(int id, ProveedorUpdateDto dto)
    {
        var proveedor = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Proveedor), id);

        if (!string.IsNullOrWhiteSpace(dto.Cuit))
        {
            var existing = await _repository.GetByCuitAsync(dto.Cuit);
            if (existing != null && existing.Id != id)
                throw new BusinessRuleException($"Ya existe un proveedor con el CUIT '{dto.Cuit}'.");
        }

        proveedor.RazonSocial = dto.RazonSocial;
        proveedor.Cuit = dto.Cuit;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;
        proveedor.Direccion = dto.Direccion;

        await _repository.UpdateAsync(proveedor);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            throw new EntityNotFoundException(nameof(Proveedor), id);

        await _repository.DeleteAsync(id);
    }
}
