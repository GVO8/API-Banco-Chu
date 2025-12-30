using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using BMPTec.Application.Interfaces.Repositories;
using BMPTec.Application.Interfaces.Services;
using BMPTec.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BMPTec.Application.Services
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly List<DateTime> _feriadosCache;
        private DateTime _ultimaAtualizacaoFeriados;

        public TransferenciaService(
            ITransferenciaRepository transferenciaRepository,
            IContaRepository contaRepository,
            IMapper mapper,
            ILogger<ContaService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _transferenciaRepository = transferenciaRepository ?? throw new ArgumentNullException(nameof(transferenciaRepository));
            _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory?.CreateClient() ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _feriadosCache = new List<DateTime>();
            _ultimaAtualizacaoFeriados = DateTime.MinValue;
        }

        public async Task<TransferenciaSaldoResponse> TransferirSaldoAsync(TransferenciaSaldoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando transferência da conta de id {ContaOrigemId} para conta de id {ContaDestinoId}", 
                    request.ContaOrigemId, request.ContaDestinoId);

                // Verificar se é dia útil
                var dataAtual = DateTime.Now;
                if (!await VerificarDiaUtilAsync(dataAtual))
                {
                    throw new InvalidOperationException($"Transferência não permitida. Data atual ({dataAtual:dd/MM/yyyy}) não é um dia útil.");
                }

                var contaOrigemExistente = await _contaRepository.GetByIdAsync(request.ContaOrigemId);
                var contaDestinoExistente = await _contaRepository.GetByIdAsync(request.ContaDestinoId);

                if (contaDestinoExistente == null || contaOrigemExistente == null)
                {
                    throw new InvalidOperationException("Conta Origem e/ou Conta Destino inexistente(s)");
                }

                // Verificar saldo suficiente na conta origem
                if (contaOrigemExistente.Saldo < request.Valor)
                {
                    throw new InvalidOperationException($"Saldo insuficiente na conta origem. Saldo atual: {contaOrigemExistente.Saldo:C}");
                }

                contaOrigemExistente.Debitar(request.Valor);
                contaDestinoExistente.Creditar(request.Valor);

                await _contaRepository.UpdateAsync(contaOrigemExistente);
                await _contaRepository.UpdateAsync(contaDestinoExistente);

                var transferencia = new Transferencia(
                    contaOrigemExistente, 
                    contaDestinoExistente, 
                    request.Valor, 
                    $"Transferência de Saldo de {contaOrigemExistente.NumeroConta} para {contaDestinoExistente.NumeroConta}");

                await _transferenciaRepository.AddAsync(transferencia);
                
                _logger.LogInformation("Transferência realizada com sucesso: {TransferenciaId}", transferencia.Id);

                var response = _mapper.Map<TransferenciaSaldoResponse>(transferencia);
                return response;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação não permitida para transferência da conta de id {ContaOrigemId} para conta de id {ContaDestinoId}", 
                    request.ContaOrigemId, request.ContaDestinoId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar transferência da conta de id {ContaOrigemId} para conta de id {ContaDestinoId}", 
                    request.ContaOrigemId, request.ContaDestinoId);
                throw;
            }
        }

        public async Task<DepositoResponse> RealizarDeposito(DepositoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando depósito para conta de id {ContaDestinoId}", request.ContaDestinoId);

                // Verificar se é dia útil para depósitos (opcional - pode remover se depósito for permitido sempre)
                var dataAtual = DateTime.Now;
                if (!await VerificarDiaUtilAsync(dataAtual))
                {
                    _logger.LogWarning("Depósito realizado em dia não útil: {Data}", dataAtual);
                    // Não lançamos exceção para depósito, apenas logamos o warning
                }

                var contaDestinoExistente = await _contaRepository.GetByIdAsync(request.ContaDestinoId);

                if (contaDestinoExistente == null)
                {
                    throw new InvalidOperationException("Conta Destino inexistente");
                }

                contaDestinoExistente.Creditar(request.Valor);

                await _contaRepository.UpdateAsync(contaDestinoExistente);

                var transferencia = new Transferencia(
                    null, 
                    contaDestinoExistente, 
                    request.Valor, 
                    $"Depósito para {contaDestinoExistente.NumeroConta}");

                await _transferenciaRepository.AddAsync(transferencia);
                _logger.LogInformation("Depósito realizado com sucesso: {TransferenciaId}", transferencia.Id);

                var response = _mapper.Map<DepositoResponse>(transferencia);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar depósito para conta de id {ContaDestinoId}", request.ContaDestinoId);
                throw;
            }
        }

        /// <summary>
        /// Verifica se a data atual é um dia útil (não é fim de semana nem feriado nacional)
        /// </summary>
        private async Task<bool> VerificarDiaUtilAsync(DateTime data)
        {
            // Verificar se é fim de semana
            if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            // Verificar se é feriado
            var feriados = await ObterFeriadosDoAnoAsync(data.Year);
            return !feriados.Any(f => f.Date == data.Date);
        }

        /// <summary>
        /// Obtém a lista de feriados nacionais para o ano especificado usando a BrasilAPI
        /// </summary>
        private async Task<List<DateTime>> ObterFeriadosDoAnoAsync(int ano)
        {
            // Verificar se temos os feriados em cache e se ainda são válidos (do mesmo ano)
            if (_feriadosCache.Any() && _ultimaAtualizacaoFeriados.Year == ano)
            {
                return _feriadosCache;
            }

            try
            {
                _logger.LogInformation("Buscando feriados nacionais para o ano {Ano}", ano);
                
                var url = $"https://brasilapi.com.br/api/feriados/v1/{ano}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var feriadosApi = JsonSerializer.Deserialize<List<FeriadoApiResponse>>(
                        jsonResponse, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (feriadosApi != null)
                    {
                        _feriadosCache.Clear();
                        _feriadosCache.AddRange(feriadosApi.Select(f => f.date));
                        _ultimaAtualizacaoFeriados = DateTime.Now;
                        
                        _logger.LogInformation("Carregados {Quantidade} feriados para o ano {Ano}", _feriadosCache.Count, ano);
                        return _feriadosCache;
                    }
                }
                else
                {
                    _logger.LogWarning("Falha ao buscar feriados. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar feriados da BrasilAPI para o ano {Ano}", ano);
                // Em caso de falha na API, assumimos que não há feriados para não bloquear as operações
            }

            // Retorna lista vazia em caso de erro
            return new List<DateTime>();
        }

        /// <summary>
        /// Classe para deserialização da resposta da BrasilAPI
        /// </summary>
        private class FeriadoApiResponse
        {
            public DateTime date { get; set; }
            public string name { get; set; } = string.Empty;
            public string type { get; set; } = string.Empty;
        }

        // Método adicional para forçar atualização do cache de feriados (útil para testes)
        public async Task AtualizarCacheFeriadosAsync(int ano)
        {
            await ObterFeriadosDoAnoAsync(ano);
        }

        // Método para obter informações sobre os feriados (útil para debug/log)
        public async Task<List<FeriadoInfoResponse>> ObterFeriadosComInformacoesAsync(int ano)
        {
            try
            {
                var url = $"https://brasilapi.com.br/api/feriados/v1/{ano}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var feriadosApi = JsonSerializer.Deserialize<List<FeriadoApiResponse>>(
                        jsonResponse, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return feriadosApi?.Select(f => new FeriadoInfoResponse
                    {
                        Data = f.date,
                        Nome = f.name,
                        Tipo = f.type
                    }).ToList() ?? new List<FeriadoInfoResponse>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar informações detalhadas de feriados");
            }

            return new List<FeriadoInfoResponse>();
        }
    }

    // Classe de resposta para informações de feriados
    public class FeriadoInfoResponse
    {
        public DateTime Data { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}