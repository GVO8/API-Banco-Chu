using System;
using System.Collections.Generic;

namespace BMPTec.Application.DTOs
{
    public class ExtratoResponse
    {
        public string NumeroConta { get; set; } = "";
        public string Agencia { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public string Periodo { get; set; } = "";
        public DateTime DataGeracao { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoAtual { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public int QuantidadeTransacoes { get; set; }
        public List<LancamentoExtratoResponse> Lancamentos { get; set; } = new();
        public ResumoExtratoResponse Resumo { get; set; } = new();
    }
}