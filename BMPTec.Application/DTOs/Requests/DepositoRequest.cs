using System;

namespace BMPTec.Application.DTOs.Requests
{
    public class DepositoRequest
    {
        public Guid ContaDestinoId { get; set; }
        public decimal Valor { get; set; }
    }
}