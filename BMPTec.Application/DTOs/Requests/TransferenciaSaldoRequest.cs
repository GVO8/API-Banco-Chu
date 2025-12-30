using System;

namespace BMPTec.Application.DTOs.Requests
{
    public class TransferenciaSaldoRequest
    {
        public Guid ContaOrigemId { get; set; }
        public Guid ContaDestinoId { get; set; }
        public decimal Valor { get; set; }
    }
}