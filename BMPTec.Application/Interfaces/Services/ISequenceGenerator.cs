using System.Threading.Tasks;

namespace BMPTec.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para geração de sequências únicas e números sequenciais
    /// </summary>
    public interface ISequenceGenerator
    {
        /// <summary>
        /// Gera o próximo número em uma sequência específica
        /// </summary>
        /// <param name="sequenceName">Nome da sequência (ex: "CONTA", "TRANSFERENCIA")</param>
        /// <returns>Próximo número da sequência</returns>
        Task<long> NextAsync(string sequenceName);
        
        /// <summary>
        /// Gera um número de conta bancária único
        /// </summary>
        Task<string> GerarNumeroContaAsync();
        
        /// <summary>
        /// Gera um código de transferência único
        /// </summary>
        Task<string> GerarCodigoTransferenciaAsync();
        
        /// <summary>
        /// Gera um número de agência bancária
        /// </summary>
        string GerarNumeroAgencia();
        
        /// <summary>
        /// Gera um dígito verificador para uma conta
        /// </summary>
        int GerarDigitoVerificador(string numeroBase);
        
        /// <summary>
        /// Reseta uma sequência específica
        /// </summary>
        Task ResetAsync(string sequenceName);
    }
}