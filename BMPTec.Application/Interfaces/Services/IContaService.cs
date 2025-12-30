using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Services
{
    public interface IContaService
    {
        Task<ContaResponse> CriarContaAsync(CriarContaRequest request);
        Task<ContaResponse> GetContaByIdAsync(Guid id);
        //Task<ContaResponse> GetContaByNumeroAsync(string numeroConta);
        //Task<SaldoResponse> GetSaldoAsync(Guid contaId);
        //Task BloquearContaAsync(Guid contaId);
        //Task DesbloquearContaAsync(Guid contaId);
        //Task EncerrarContaAsync(Guid contaId);
    }
}