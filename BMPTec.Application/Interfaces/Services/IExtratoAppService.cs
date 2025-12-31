using BMPTec.Application.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Services
{
    public interface IExtratoAppService
    {
        Task<ExtratoResponse> GerarExtratoAsync(ExtratoRequest request);
        Task<MemoryStream> GerarExtratoTxtAsync(ExtratoRequest request);
    }
}