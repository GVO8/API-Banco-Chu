using System;
using System.Text.Json.Serialization;
using BMPTec.Domain.Entities;
using BMPTec.Domain.Enums;

namespace BMPTec.Application.DTOs.Responses
{
    public class ContaResponse
    {
        public Guid Id { get; set; }
        public string NumeroConta { get; set; } = "";
        public string Agencia { get; set; } = "";
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoConta TipoConta { get; set; }
        
        public decimal Saldo { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusConta Status { get; set; }
        
        public DateTime DataAbertura { get; set; }
        public ClienteResponse Cliente { get; set; } = new ClienteResponse();
    }
}