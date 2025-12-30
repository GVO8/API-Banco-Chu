using System;

namespace BMPTec.Application.DTOs.Responses
{
    public class ClienteResponse
    {
        public string Nome { get; set; } = "";
        public string CPF { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = "";
        public bool Ativo { get; set; }
    }
}