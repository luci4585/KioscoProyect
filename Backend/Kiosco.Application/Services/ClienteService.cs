using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();
        return clientes.Select(c => c.ToDto());
    }

    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        return cliente?.ToDto();
    }

    public async Task<IEnumerable<ClienteDto>> SearchAsync(string? nombre, string? documento)
    {
        var clientes = await _repository.SearchAsync(nombre, documento);
        return clientes.Select(c => c.ToDto());
    }

    public async Task<ClienteDto> CreateAsync(ClienteCreateDto dto)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Documento = dto.Documento,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion
        };

        var created = await _repository.CreateAsync(cliente);
        return created.ToDto();
    }

    public async Task UpdateAsync(int id, ClienteUpdateDto dto)
    {
        var cliente = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Cliente), id);

        cliente.Nombre = dto.Nombre;
        cliente.Apellido = dto.Apellido;
        cliente.Documento = dto.Documento;
        cliente.Telefono = dto.Telefono;
        cliente.Email = dto.Email;
        cliente.Direccion = dto.Direccion;

        await _repository.UpdateAsync(cliente);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            throw new EntityNotFoundException(nameof(Cliente), id);

        await _repository.DeleteAsync(id);
    }
}
