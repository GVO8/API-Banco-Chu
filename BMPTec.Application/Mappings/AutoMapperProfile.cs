using System;
using AutoMapper;
using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using BMPTec.Domain.Entities;

namespace ChuBank.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ========== REQUESTS PARA ENTIDADES ==========
            
            // CriarContaRequest -> Cliente
            CreateMap<CriarContaRequest, Cliente>()
                .ConstructUsing(src => new Cliente(
                    src.Nome,
                    src.CPF,
                    src.Email,
                    src.DataNascimento,
                    src.Telefone))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Contas, opt => opt.Ignore())
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(_ => true));

            //// TransferenciaRequest -> Transferencia (com validações)
            //CreateMap<TransferenciaRequest, Transferencia>()
            //    .ConstructUsing((src, context) =>
            //    {
            //        var contaOrigem = context.Items["ContaOrigem"] as Conta;
            //        var contaDestino = context.Items["ContaDestino"] as Conta;
            //        
            //        return new Transferencia(
            //            contaOrigem,
            //            contaDestino,
            //            src.Valor,
            //            src.Descricao);
            //    })
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => StatusTransferencia.Pendente))
            //    .ForMember(dest => dest.DataSolicitacao, opt => opt.MapFrom(_ => DateTime.UtcNow))
            //    .ForMember(dest => dest.DataProcessamento, opt => opt.Ignore())
            //    .ForMember(dest => dest.CodigoRastreio, opt => opt.MapFrom(_ => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()));

            // ========== ENTIDADES PARA RESPONSES ==========
            
            // Cliente -> ClienteResponse
            CreateMap<Cliente, ClienteResponse>()
                .ForMember(dest => dest.CPF, 
                    opt => opt.MapFrom(src => src.CPF))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Nome,
                    opt => opt.MapFrom(src => FormatNome(src.Nome)));

            CreateMap<Conta, ContaResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Guid para Guid - OK
                .ForMember(dest => dest.NumeroConta, opt => opt.Ignore()) // Será configurado no AfterMap
                .ForMember(dest => dest.Agencia, opt => opt.MapFrom(src => src.Agencia))
                .ForMember(dest => dest.TipoConta, opt => opt.MapFrom(src => src.TipoConta))
                .ForMember(dest => dest.Saldo, opt => opt.MapFrom(src => Math.Round(src.Saldo, 2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.DataAbertura, opt => opt.MapFrom(src => src.DataAbertura.ToLocalTime()))
                .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente))
                .AfterMap((src, dest) =>
                {
                    // Formatar número da conta para exibição
                    dest.NumeroConta = FormatNumeroConta(src.NumeroConta);
                });

            //// Transferencia -> TransferenciaResponse
            //CreateMap<Transferencia, TransferenciaResponse>()
            //    .ForMember(dest => dest.NumeroContaOrigem,
            //        opt => opt.MapFrom(src => src.ContaOrigem.NumeroConta))
            //    .ForMember(dest => dest.NomeClienteOrigem,
            //        opt => opt.MapFrom(src => src.ContaOrigem.Cliente.Nome))
            //    .ForMember(dest => dest.NumeroContaDestino,
            //        opt => opt.MapFrom(src => src.ContaDestino.NumeroConta))
            //    .ForMember(dest => dest.NomeClienteDestino,
            //        opt => opt.MapFrom(src => src.ContaDestino.Cliente.Nome))
            //    .ForMember(dest => dest.Valor,
            //        opt => opt.MapFrom(src => Math.Round(src.Valor, 2)))
            //    .ForMember(dest => dest.DataSolicitacao,
            //        opt => opt.MapFrom(src => src.DataSolicitacao.ToLocalTime()))
            //    .ForMember(dest => dest.DataProcessamento,
            //        opt => opt.MapFrom(src => src.DataProcessamento.HasValue 
            //            ? src.DataProcessamento.Value.ToLocalTime() 
            //            : (DateTime?)null));

            // ========== MAPEAMENTOS COMPLEXOS ==========
            
            //// Conta para ExtratoResponse (com transações agrupadas)
            //CreateMap<Conta, ExtratoResponse>()
            //    .ForMember(dest => dest.NumeroConta,
            //        opt => opt.MapFrom(src => src.NumeroConta))
            //    .ForMember(dest => dest.SaldoAtual,
            //        opt => opt.MapFrom(src => Math.Round(src.Saldo, 2)))
            //    .ForMember(dest => dest.DataInicio,
            //        opt => opt.Ignore())
            //    .ForMember(dest => dest.DataFim,
            //        opt => opt.Ignore())
            //    .ForMember(dest => dest.Transacoes,
            //        opt => opt.MapFrom(src => src.Transacoes.OrderByDescending(t => t.DataTransacao)));

            //// Paginação genérica
            //CreateMap(typeof(PagedList<>), typeof(PaginatedResponse<>))
            //    .ConvertUsing(typeof(PagedListConverter<,>));

            //// ========== MAPEAMENTOS CUSTOMIZADOS ==========
            //
            //// Feriado da BrasilAPI para FeriadoCache
            //CreateMap<FeriadoResponse, FeriadoCache>()
            //    .ForMember(dest => dest.Id,
            //        opt => opt.MapFrom(src => Guid.NewGuid()))
            //    .ForMember(dest => dest.Data,
            //        opt => opt.MapFrom(src => DateOnly.FromDateTime(src.Date)))
            //    .ForMember(dest => dest.Nome,
            //        opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.Tipo,
            //        opt => opt.MapFrom(src => src.Type))
            //    .ForMember(dest => dest.CacheUntil,
            //        opt => opt.MapFrom(src => DateTime.UtcNow.AddHours(24)));
        }

        // ========== MÉTODOS AUXILIARES PRIVADOS ==========
        
        private string FormatNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return nome;
                
            var nomes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var nomeFormatado = nomes[0];
            
            if (nomes.Length > 1)
            {
                nomeFormatado += " " + nomes[nomes.Length - 1];
            }
            
            return nomeFormatado;
        }

        private string FormatNumeroConta(string numeroConta)
        {
            if (string.IsNullOrWhiteSpace(numeroConta) || numeroConta.Length != 7)
                return numeroConta;
                
            // Formata 123456-7 para 12345.6-7
            return $"{numeroConta.Substring(0, 5)}.{numeroConta.Substring(5, 1)}-{numeroConta.Substring(6, 1)}";
        }
    }
}