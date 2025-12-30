using BMPTec.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Repositories
{
    public interface IContaRepository
    {
        Task<Conta> GetByIdAsync(Guid id);
        Task<Conta> GetByNumeroContaAsync(string numeroConta);
        Task<IEnumerable<Conta>> GetAllAsync();
        Task<IEnumerable<Conta>> GetByClienteIdAsync(Guid clienteId);
        Task<Conta> AddAsync(Conta conta);
        Task UpdateAsync(Conta conta);
        Task DeleteAsync(Conta conta);
        Task<bool> ExistsAsync(string numeroConta);
        Task<Cliente> GetClienteByCpfAsync(string cpf);
        Task<Cliente> AddClienteAsync(Cliente cliente);
    }
}