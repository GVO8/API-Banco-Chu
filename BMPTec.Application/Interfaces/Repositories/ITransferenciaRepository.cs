using BMPTec.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Repositories
{
    public interface ITransferenciaRepository
    {
        Task<Transferencia> GetByIdAsync(Guid id);
        Task<IEnumerable<Transferencia>> GetAllAsync();
        Task<Transferencia> AddAsync(Transferencia transferencia);
        Task<bool> ExistsAsync(Guid id);
    }
}
