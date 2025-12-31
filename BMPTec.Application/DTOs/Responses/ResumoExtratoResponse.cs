namespace BMPTec.Application.DTOs
{
   public class ResumoExtratoResponse
    {
        public decimal MediaDiariaCreditos { get; set; }
        public decimal MediaDiariaDebitos { get; set; }
        public int DiasComTransacoes { get; set; }
        public string DiaMaisMovimentado { get; set; } = "";
        public decimal MaiorCredito { get; set; }
        public decimal MaiorDebito { get; set; }
    }
}