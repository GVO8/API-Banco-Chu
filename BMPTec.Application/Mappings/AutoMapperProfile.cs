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

            // Transferencia -> TransferenciaResponse
            CreateMap<Transferencia, TransferenciaSaldoResponse>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Descricao,
                    opt => opt.MapFrom(src => src.Descricao))
                .ForMember(dest => dest.CodigoRastreio,
                    opt => opt.MapFrom(src => src.CodigoRastreio))
                .ForMember(dest => dest.ComprovanteUrl,
                    opt => opt.MapFrom(src => src.ComprovanteUrl))
                .ForMember(dest => dest.Taxa,
                    opt => opt.MapFrom(src => src.Taxa))
                .ForMember(dest => dest.ContaOrigemId,
                    opt => opt.MapFrom(src => src.ContaOrigemId))
                .ForMember(dest => dest.ContaDestinoId,
                    opt => opt.MapFrom(src => src.ContaDestinoId))
                .ForMember(dest => dest.Valor,
                    opt => opt.MapFrom(src => Math.Round(src.Valor, 2)))
                .ForMember(dest => dest.DataSolicitacao,
                    opt => opt.MapFrom(src => src.DataSolicitacao.ToLocalTime()))
                .ForMember(dest => dest.DataProcessamento,
                    opt => opt.MapFrom(src => src.DataProcessamento.HasValue 
                        ? src.DataProcessamento.Value.ToLocalTime() 
                        : (DateTime?)null));

                // Transferencia -> TransferenciaResponse
            CreateMap<Transferencia, DepositoResponse>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Descricao,
                    opt => opt.MapFrom(src => src.Descricao))
                .ForMember(dest => dest.CodigoRastreio,
                    opt => opt.MapFrom(src => src.CodigoRastreio))
                .ForMember(dest => dest.ComprovanteUrl,
                    opt => opt.MapFrom(src => src.ComprovanteUrl))
                .ForMember(dest => dest.ContaDestinoId,
                    opt => opt.MapFrom(src => src.ContaDestinoId))
                .ForMember(dest => dest.Valor,
                    opt => opt.MapFrom(src => Math.Round(src.Valor, 2)))
                .ForMember(dest => dest.DataSolicitacao,
                    opt => opt.MapFrom(src => src.DataSolicitacao.ToLocalTime()))
                .ForMember(dest => dest.DataProcessamento,
                    opt => opt.MapFrom(src => src.DataProcessamento.HasValue 
                        ? src.DataProcessamento.Value.ToLocalTime() 
                        : (DateTime?)null));
        }

        // Relacionamentos
        public Guid? ContaOrigemId { get; private set; }
        public Guid ContaDestinoId { get; private set; }

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