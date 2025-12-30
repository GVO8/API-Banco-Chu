using BMPTec.Application.Interfaces.Repositories;
using BMPTec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMPTec.Infrastructure.Data.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        
        private readonly AppDbContext _context;

        public TransferenciaRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Transferencia> GetByIdAsync(Guid id)
        {
            return await _context.Transferencia
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transferencia>> GetAllAsync()
        {
            return await _context.Transferencia.ToListAsync();
        }

        public async Task<Transferencia> AddAsync(Transferencia transferencia)
        {
            await _context.Transferencia.AddAsync(transferencia);
            await _context.SaveChangesAsync();
            return transferencia;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Transferencia.AnyAsync(t => t.Id == id);
        }
    }
}