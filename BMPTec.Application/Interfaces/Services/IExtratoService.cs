using System.IO;
using System.Threading.Tasks;
using BMPTec.Application.DTOs;

namespace BMPTec.Application.Interfaces.Services
{
    public interface IExtratoService
    {
        Task<ExtratoResponse> GerarExtratoAsync(ExtratoRequest request);
        Task<MemoryStream> GerarExtratoPdfAsync(ExtratoRequest request);
    }
}