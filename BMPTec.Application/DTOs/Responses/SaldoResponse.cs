using System;

namespace BMPTec.Application.DTOs.Responses
{
    public class SaldoResponse
    {
        public Guid ContaId { get; set; }
        public string NumeroConta { get; set; } = "";
        public decimal Saldo { get; set; }
        public DateTime DataConsulta { get; set; }
    }
}