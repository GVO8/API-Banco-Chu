using System;
using System.IO;
using System.Threading.Tasks;
using BMPTec.Application.DTOs;
using BMPTec.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;


namespace BMPTec.Application.Services
{
    public class ExtratoAppService : IExtratoAppService
    {
        private readonly IExtratoService _extratoService;
        private readonly ILogger<ExtratoAppService> _logger;

        public ExtratoAppService(
            IExtratoService extratoService,
            ILogger<ExtratoAppService> logger)
        {
            _extratoService = extratoService;
            _logger = logger;
        }

        public async Task<ExtratoResponse> GerarExtratoAsync(ExtratoRequest request)
        {
            try
            {
                // Adicionar lógica de aplicação/business aqui
                _logger.LogInformation("Iniciando geração de extrato para conta {ContaId}", request.ContaId);
                
                // Validar regras de negócio específicas da aplicação
                await ValidarRegrasNegocioAsync(request);
                
                // Delegar para o service de infra
                var extrato = await _extratoService.GerarExtratoAsync(request);
                
                // Processar/adicionar dados da aplicação
                extrato.DataGeracao = DateTime.UtcNow;
                
                _logger.LogInformation("Extrato gerado com {Transacoes} transações", extrato.QuantidadeTransacoes);
                
                return extrato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no serviço de aplicação");
                throw;
            }
        }

        private async Task ValidarRegrasNegocioAsync(ExtratoRequest request)
        {
            // Exemplo: Verificar se cliente tem permissão para ver extrato
            // Verificar limites de período
            // Aplicar políticas de segurança, etc.
            
            var diasPeriodo = (request.DataFim - request.DataInicio).TotalDays;
            if (diasPeriodo > 90)
                throw new InvalidOperationException("Período máximo para extrato é 90 dias");
        }

        public async Task<MemoryStream> GerarExtratoTxtAsync(ExtratoRequest request)
        {
            // Adicionar formatação específica da aplicação
            var extrato = await GerarExtratoAsync(request);
            
            // Poderia adicionar cabeçalho/rodapé específico
            // Ou aplicar formatação de acordo com configuração do cliente
            
            return await _extratoService.GerarExtratoPdfAsync(request);
        }
    }
}