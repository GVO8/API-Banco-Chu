using System;

namespace BMPTec.Application.DTOs
{
    public class ExtratoRequest
    {
        public Guid ContaId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int? Pagina { get; set; } = 1;
        public int? TamanhoPagina { get; set; } = 50;
        
        public bool Validar()
        {
            if (DataInicio > DataFim)
                throw new ArgumentException("Data início não pode ser maior que data fim");
                
            if ((DataFim - DataInicio).TotalDays > 365)
                throw new ArgumentException("Período máximo é de 365 dias");
                
            return true;
        }
    }
}