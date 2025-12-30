using BMPTec.Application.Interfaces.Repositories;
using BMPTec.Domain.Entities;
using BMPTec.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMPTec.Infrastructure.Data.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly AppDbContext _context;

        public ContaRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Conta> GetByIdAsync(Guid id)
        {
            return await _context.Conta
                .Include(c => c.Cliente)
                .Include(c => c.Transferencias)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Conta> GetByNumeroContaAsync(string numeroConta)
        {
            return await _context.Conta
                .Include(c => c.Cliente)
                .Include(c => c.Transferencias)
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);
        }

        public async Task<IEnumerable<Conta>> GetAllAsync()
        {
            return await _context.Conta
                .Include(c => c.Cliente)
                .Where(c => c.Status != StatusConta.Encerrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Conta>> GetByClienteIdAsync(Guid clienteId)
        {
            return await _context.Conta
                .Include(c => c.Cliente)
                .Where(c => c.ClienteId == clienteId && c.Status != StatusConta.Encerrada)
                .ToListAsync();
        }

        public async Task<Conta> AddAsync(Conta conta)
        {
            await _context.Conta.AddAsync(conta);
            await _context.SaveChangesAsync();
            return conta;
        }

        public async Task UpdateAsync(Conta conta)
        {
            _context.Conta.Update(conta);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Conta conta)
        {
            _context.Conta.Remove(conta);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string numeroConta)
        {
            return await _context.Conta.AnyAsync(c => c.NumeroConta == numeroConta);
        }

        public async Task<Cliente> GetClienteByCpfAsync(string cpf)
        {   
            return await _context.Cliente
                .FirstOrDefaultAsync(c => c.CPF == cpf);
        }

        public async Task<Cliente> AddClienteAsync(Cliente cliente)
        {
            await _context.Cliente.AddAsync(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }
    }
}