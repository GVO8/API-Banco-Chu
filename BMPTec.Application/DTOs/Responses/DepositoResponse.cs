using System;

namespace BMPTec.Application.DTOs.Responses
{
    public class DepositoResponse
    {
        public Guid Id { get; set; }
        public decimal Valor { get; private set; }
        public string Descricao { get; private set; } = "";
        public DateTime DataSolicitacao { get; private set; }
        public DateTime? DataProcessamento { get; private set; }
        public string CodigoRastreio { get; private set; } = "";
        public string ComprovanteUrl { get; private set; } = "";

        // Relacionamentos
        public Guid ContaDestinoId { get; private set; }
    }
}