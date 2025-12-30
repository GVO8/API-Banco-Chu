using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using BMPTec.Application.Interfaces.Repositories;
using BMPTec.Application.Interfaces.Services;
using BMPTec.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BMPTec.Application.Services
{
    public class TransferenciaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContaService> _logger;

        public TransferenciaService(
            ITransferenciaRepository transferenciaRepository,
            IContaRepository contaRepository,
            IMapper mapper,
            ILogger<ContaService> logger)
        {
            _transferenciaRepository = transferenciaRepository ?? throw new ArgumentNullException(nameof(transferenciaRepository));
            _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransferenciaSaldoResponse> TransferirSaldoAsync(TransferenciaSaldoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando transferência da conta de id {ContaOrigemId} para conta de id {ContaDestinoId}", request.ContaOrigemId, request.ContaDestinoId);

                var contaOrigemExistente = await _contaRepository.GetByIdAsync(request.ContaOrigemId);
                var contaDestinoExistente = await _contaRepository.GetByIdAsync(request.ContaDestinoId);

                if (contaDestinoExistente == null || contaOrigemExistente == null)
                {
                    throw new InvalidOperationException("Conta Origem e/ou Conta Destino inexistente(s)");
                }

                var transferencia = new Transferencia(
                    contaOrigemExistente, 
                    contaDestinoExistente, 
                    request.Valor, 
                    $"Transferência de Saldo de {contaOrigemExistente} para {contaDestinoExistente}");

                await _transferenciaRepository.AddAsync(transferencia);
                _logger.LogInformation("Transferência realizada com sucesso: {TransferenciaId}", transferencia.Id);

                var response = _mapper.Map<TransferenciaSaldoResponse>(transferencia);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar transferência da conta de id {ContaOrigemId} para conta de id {ContaDestinoId}", request.ContaOrigemId, request.ContaDestinoId);
                throw;
            }
        }

        public async Task<DepositoResponse> RealizarDeposito(DepositoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando depósito para conta de id {ContaDestinoId}", request.ContaDestinoId);

                var contaDestinoExistente = await _contaRepository.GetByIdAsync(request.ContaDestinoId);

                if (contaDestinoExistente == null)
                {
                    throw new InvalidOperationException("Conta Destino inexistente");
                }

                var transferencia = new Transferencia(
                    null, 
                    contaDestinoExistente, 
                    request.Valor, 
                    $"Depósito para {contaDestinoExistente}");

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
    }
}