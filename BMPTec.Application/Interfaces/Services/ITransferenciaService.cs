using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Services
{
    public interface ITransferenciaService
    {
        Task<TransferenciaSaldoResponse> TransferirSaldoAsync(TransferenciaSaldoRequest request);
        Task<DepositoResponse> RealizarDeposito(DepositoRequest request);
    }
}