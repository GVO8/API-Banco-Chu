using System;

namespace BMPTec.Application.DTOs
{
    public class LancamentoExtratoResponse
    {
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public decimal SaldoAposTransacao { get; set; }
        public string? CodigoRastreio { get; set; }
        public string? Contraparte { get; set; }
    }
}