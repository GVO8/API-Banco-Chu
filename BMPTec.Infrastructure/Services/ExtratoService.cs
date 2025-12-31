using Microsoft.EntityFrameworkCore;
using BMPTec.Application.DTOs;
using BMPTec.Domain.Entities;
using BMPTec.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using BMPTec.Domain.Enums;
using BMPTec.Application.Interfaces.Services;

namespace BMPTec.Infrastructure.Services
{
    public class ExtratoService : IExtratoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExtratoService> _logger;

        public ExtratoService(
            AppDbContext context,
            ILogger<ExtratoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ExtratoResponse> GerarExtratoAsync(ExtratoRequest request)
        {
            try
            {
                request.Validar();
                
                // Buscar conta com cliente
                var conta = await _context.Conta
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.Id == request.ContaId && c.Status == StatusConta.Ativa);
                    
                if (conta == null)
                    throw new KeyNotFoundException("Conta não encontrada ou inativa");

                // Buscar todas as transferências relacionadas à conta no período
                var transferencias = await _context.Transferencia
                    .Include(t => t.ContaOrigem)
                    .Include(t => t.ContaDestino)
                    .Where(t => (t.ContaOrigemId == request.ContaId || t.ContaDestinoId == request.ContaId) &&
                               t.DataSolicitacao >= request.DataInicio &&
                               t.DataSolicitacao <= request.DataFim)
                    .OrderByDescending(t => t.DataSolicitacao)
                    .ToListAsync();

                // Buscar saldo anterior (último saldo antes do período)
                var saldoAnterior = await CalcularSaldoAnteriorAsync(request.ContaId, request.DataInicio);
                var saldoAtual = conta.Saldo;

                // Transformar transferências em lançamentos
                var lancamentos = await TransformarTransferenciasEmLancamentosAsync(
                    transferencias, conta.Id, saldoAnterior);

                // Calcular totais
                var totalCreditos = lancamentos
                    .Where(l => l.Tipo == "CREDITO")
                    .Sum(l => l.Valor);
                    
                var totalDebitos = lancamentos
                    .Where(l => l.Tipo == "DEBITO")
                    .Sum(l => l.Valor);

                // Gerar resumo
                var resumo = await GerarResumoAsync(lancamentos, request);

                // Montar extrato
                var extrato = new ExtratoResponse
                {
                    NumeroConta = conta.NumeroConta,
                    Agencia = conta.Agencia,
                    NomeCliente = conta.Cliente.Nome,
                    Periodo = $"{request.DataInicio:dd/MM/yyyy} a {request.DataFim:dd/MM/yyyy}",
                    DataGeracao = DateTime.UtcNow,
                    SaldoAnterior = saldoAnterior,
                    SaldoAtual = saldoAtual,
                    TotalCreditos = totalCreditos,
                    TotalDebitos = totalDebitos,
                    QuantidadeTransacoes = lancamentos.Count,
                    Lancamentos = lancamentos,
                    Resumo = resumo
                };

                // Log de auditoria
                _logger.LogInformation(
                    "Extrato gerado para conta {ContaId} no período {DataInicio} a {DataFim}",
                    request.ContaId, request.DataInicio, request.DataFim);

                return extrato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar extrato");
                throw;
            }
        }

        private async Task<decimal> CalcularSaldoAnteriorAsync(Guid contaId, DateTime dataInicio)
        {
            // Buscar última transferência antes do período
            var ultimaTransferencia = await _context.Transferencia
                .Where(t => (t.ContaOrigemId == contaId || t.ContaDestinoId == contaId) &&
                           t.DataSolicitacao < dataInicio)
                .OrderByDescending(t => t.DataSolicitacao)
                .FirstOrDefaultAsync();

            if (ultimaTransferencia == null)
            {
                // Se não houver transferências anteriores, buscar saldo da conta no momento da abertura
                var conta = await _context.Conta.FindAsync(contaId);
                return conta?.Saldo ?? 0;
            }

            // Calcular saldo baseado nas transferências anteriores
            return await CalcularSaldoNaDataAsync(contaId, dataInicio);
        }

        private async Task<decimal> CalcularSaldoNaDataAsync(Guid contaId, DateTime data)
        {
            var transferencias = await _context.Transferencia
                .Where(t => (t.ContaOrigemId == contaId || t.ContaDestinoId == contaId) &&
                           t.DataSolicitacao < data)
                .ToListAsync();

            var saldo = 0m;

            foreach (var transferencia in transferencias)
            {
                if (transferencia.ContaOrigemId == contaId)
                    saldo -= transferencia.Valor + (transferencia.Taxa ?? 0);
                else if (transferencia.ContaDestinoId == contaId)
                    saldo += transferencia.Valor;
            }

            return saldo;
        }

        private async Task<List<LancamentoExtratoResponse>> TransformarTransferenciasEmLancamentosAsync(
            List<Transferencia> transferencias, Guid contaId, decimal saldoAnterior)
        {
            var lancamentos = new List<LancamentoExtratoResponse>();
            var saldoAcumulado = saldoAnterior;

            foreach (var transferencia in transferencias.OrderBy(t => t.DataSolicitacao))
            {
                var lancamento = new LancamentoExtratoResponse
                {
                    Data = transferencia.DataSolicitacao,
                    CodigoRastreio = transferencia.CodigoRastreio,
                    Descricao = transferencia.Descricao
                };

                if (transferencia.ContaOrigemId == contaId)
                {
                    // Saída/Débito
                    lancamento.Tipo = "DEBITO";
                    lancamento.Valor = transferencia.Valor + (transferencia.Taxa ?? 0);
                    lancamento.Contraparte = transferencia.ContaDestino?.NumeroConta;
                    saldoAcumulado -= lancamento.Valor;
                }
                else if (transferencia.ContaDestinoId == contaId)
                {
                    // Entrada/Crédito
                    lancamento.Tipo = "CREDITO";
                    lancamento.Valor = transferencia.Valor;
                    lancamento.Contraparte = transferencia.ContaOrigem?.NumeroConta ?? "EXTERNA";
                    saldoAcumulado += lancamento.Valor;
                }

                lancamento.SaldoAposTransacao = saldoAcumulado;
                lancamentos.Add(lancamento);
            }

            return lancamentos.OrderByDescending(l => l.Data).ToList();
        }

        private async Task<ResumoExtratoResponse> GerarResumoAsync(
            List<LancamentoExtratoResponse> lancamentos, ExtratoRequest request)
        {
            if (!lancamentos.Any())
                return new ResumoExtratoResponse();

            var diasComTransacoes = lancamentos
                .Select(l => l.Data.Date)
                .Distinct()
                .Count();

            var diasNoPeriodo = (request.DataFim - request.DataInicio).TotalDays + 1;

            var creditosPorDia = lancamentos
                .Where(l => l.Tipo == "CREDITO")
                .GroupBy(l => l.Data.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Total = g.Sum(x => x.Valor),
                    Quantidade = g.Count()
                })
                .ToList();

            var debitosPorDia = lancamentos
                .Where(l => l.Tipo == "DEBITO")
                .GroupBy(l => l.Data.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Total = g.Sum(x => x.Valor),
                    Quantidade = g.Count()
                })
                .ToList();

            var diaMaisMovimentado = lancamentos
                .GroupBy(l => l.Data.Date)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var maiorCredito = lancamentos
                .Where(l => l.Tipo == "CREDITO")
                .Select(l => l.Valor)
                .DefaultIfEmpty(0)
                .Max();

            var maiorDebito = lancamentos
                .Where(l => l.Tipo == "DEBITO")
                .Select(l => l.Valor)
                .DefaultIfEmpty(0)
                .Max();

            return new ResumoExtratoResponse
            {
                MediaDiariaCreditos = creditosPorDia.Any() ? 
                    creditosPorDia.Average(g => g.Total) : 0,
                MediaDiariaDebitos = debitosPorDia.Any() ? 
                    debitosPorDia.Average(g => g.Total) : 0,
                DiasComTransacoes = diasComTransacoes,
                DiaMaisMovimentado = diaMaisMovimentado?.Key.ToString("dd/MM/yyyy") ?? "N/A",
                MaiorCredito = maiorCredito,
                MaiorDebito = maiorDebito
            };
        }

        public async Task<MemoryStream> GerarExtratoPdfAsync(ExtratoRequest filtro)
        {
            var extrato = await GerarExtratoAsync(filtro);
            return await GerarArquivoTxt(extrato);
        }

        private async Task<MemoryStream> GerarArquivoTxt(ExtratoResponse extrato)
        {
            // Implementação simplificada - você pode usar bibliotecas como QuestPDF, iTextSharp, etc.
            var memoryStream = new MemoryStream();
            
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            {
                await writer.WriteLineAsync($"EXTRATO BANCÁRIO");
                await writer.WriteLineAsync($"========================================");
                await writer.WriteLineAsync($"Conta: {extrato.NumeroConta} Agência: {extrato.Agencia}");
                await writer.WriteLineAsync($"Cliente: {extrato.NomeCliente}");
                await writer.WriteLineAsync($"Período: {extrato.Periodo}");
                await writer.WriteLineAsync($"Data Geração: {extrato.DataGeracao:dd/MM/yyyy HH:mm}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"SALDO ANTERIOR: R$ {extrato.SaldoAnterior:N2}");
                await writer.WriteLineAsync($"SALDO ATUAL: R$ {extrato.SaldoAtual:N2}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"LANÇAMENTOS:");
                await writer.WriteLineAsync($"========================================");
                
                foreach (var lancamento in extrato.Lancamentos)
                {
                    await writer.WriteLineAsync(
                        $"{lancamento.Data:dd/MM/yyyy HH:mm} | " +
                        $"{lancamento.Tipo,-10} | " +
                        $"{lancamento.Descricao,-30} | " +
                        $"R$ {lancamento.Valor,10:N2} | " +
                        $"Saldo: R$ {lancamento.SaldoAposTransacao,10:N2}");
                }
                
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"RESUMO:");
                await writer.WriteLineAsync($"========================================");
                await writer.WriteLineAsync($"Total Créditos: R$ {extrato.TotalCreditos:N2}");
                await writer.WriteLineAsync($"Total Débitos: R$ {extrato.TotalDebitos:N2}");
                await writer.WriteLineAsync($"Transações: {extrato.QuantidadeTransacoes}");
                await writer.WriteLineAsync($"Dias com transações: {extrato.Resumo.DiasComTransacoes}");
                await writer.WriteLineAsync($"Dia mais movimentado: {extrato.Resumo.DiaMaisMovimentado}");
                await writer.WriteLineAsync($"Maior crédito: R$ {extrato.Resumo.MaiorCredito:N2}");
                await writer.WriteLineAsync($"Maior débito: R$ {extrato.Resumo.MaiorDebito:N2}");
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}