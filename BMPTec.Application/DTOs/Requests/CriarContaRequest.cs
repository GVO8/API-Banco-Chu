using System;
using System.Text.Json.Serialization;
using BMPTec.Domain.Enums;

namespace BMPTec.Application.DTOs.Requests
{
    public class CriarContaRequest
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoConta TipoConta { get; set; } = TipoConta.Corrente;
        
        public decimal SaldoInicial { get; set; } = 0;
    }
}