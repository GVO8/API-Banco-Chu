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
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContaService> _logger;
        private readonly ISequenceGenerator _sequenceGenerator;

        public ContaService(
            IContaRepository contaRepository,
            IMapper mapper,
            ILogger<ContaService> logger,
            ISequenceGenerator sequenceGenerator)
        {
            _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sequenceGenerator = sequenceGenerator ?? throw new ArgumentNullException(nameof(sequenceGenerator));
        }

        public async Task<ContaResponse> CriarContaAsync(CriarContaRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de conta para CPF: {CPF}", request.CPF);
                
                // Verificar se cliente já existe
                var clienteExistente = await _contaRepository.GetClienteByCpfAsync(request.CPF);
                
                Cliente cliente;
                if (clienteExistente != null)
                {
                    _logger.LogInformation("Cliente já cadastrado com CPF: {CPF}", request.CPF);
                    
                    if (!clienteExistente.Ativo)
                        throw new InvalidOperationException("Cliente está inativo");
                        
                    cliente = clienteExistente;
                }
                else
                {
                    // Criar novo cliente
                    cliente = new Cliente(
                        request.Nome,
                        request.CPF,
                        request.Email,
                        request.DataNascimento,
                        request.Telefone);
                    
                    await _contaRepository.AddClienteAsync(cliente);
                    _logger.LogInformation("Novo cliente criado: {ClienteId}", cliente.Id);
                }

                // Gerar número da conta único
                var numeroConta = await GerarNumeroContaUnicoAsync();
                
                // Criar conta
                var conta = new Conta(
                    numeroConta,
                    "0001", // Agência padrão
                    request.TipoConta,
                    cliente,
                    request.SaldoInicial);

                // Adicionar transação inicial se houver saldo
                if (request.SaldoInicial > 0)
                {
                    conta.Creditar(request.SaldoInicial);
                }

                // Salvar conta
                await _contaRepository.AddAsync(conta);
                
                _logger.LogInformation("Conta criada com sucesso: {NumeroConta}", conta.NumeroConta);
                
                // Retornar resposta
                var response = _mapper.Map<ContaResponse>(conta);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta para CPF: {CPF}", request.CPF);
                throw;
            }
        }

        public async Task<ContaResponse> GetContaByIdAsync(Guid id)
        {
            var conta = await _contaRepository.GetByIdAsync(id);
            if (conta == null)
                throw new KeyNotFoundException($"Conta não encontrada: {id}");
                
            return _mapper.Map<ContaResponse>(conta);
        }

        private async Task<string> GerarNumeroContaUnicoAsync()
        {
            string numeroConta;
            bool contaExiste;
            int tentativas = 0;
            const int maxTentativas = 10;

            do
            {
                numeroConta = await _sequenceGenerator.GerarNumeroContaAsync();
                contaExiste = await _contaRepository.ExistsAsync(numeroConta);
                tentativas++;
                
                if (tentativas >= maxTentativas)
                    throw new InvalidOperationException("Não foi possível gerar um número de conta único");
                    
            } while (contaExiste);

            return numeroConta;
        }
    }
}