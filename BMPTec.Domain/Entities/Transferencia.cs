using BMPTec.Domain.Entities.Base;
using BMPTec.Domain.Enums;

namespace BMPTec.Domain.Entities
{
    public class Transferencia : AuditableEntity
    {
        protected Transferencia() { }

        public Transferencia(
            Conta contaOrigem,
            Conta contaDestino,
            decimal valor,
            string descricao)
        {
            //ContaOrigem = contaOrigem ?? throw new ArgumentNullException(nameof(contaOrigem));
            ContaOrigemId = contaOrigem == null ? null : contaOrigem.Id;
            
            ContaDestino = contaDestino ?? throw new ArgumentNullException(nameof(contaDestino));
            ContaDestinoId = contaDestino.Id;
            
            Valor = valor > 0 ? valor : throw new ArgumentException("Valor deve ser positivo", nameof(valor));
            Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
            DataSolicitacao = DateTime.UtcNow;
            Status = StatusTransferencia.Pendente;
            CodigoRastreio = GerarCodigoRastreio();

            Validar();
        }

        // Propriedades
        public decimal Valor { get; private set; }
        public string Descricao { get; private set; }
        public DateTime DataSolicitacao { get; private set; }
        public DateTime? DataProcessamento { get; private set; }
        public StatusTransferencia Status { get; private set; }
        public string CodigoRastreio { get; private set; }
        public string ComprovanteUrl { get; private set; } = "";
        public decimal? Taxa { get; private set; }

        // Relacionamentos
        public Guid? ContaOrigemId { get; private set; }
        public virtual Conta? ContaOrigem { get; private set; }
        
        public Guid ContaDestinoId { get; private set; }
        public virtual Conta ContaDestino { get; private set; }

        // Métodos de negócio
        public void Concluir(decimal? taxa = null)
        {
            if (Status != StatusTransferencia.Pendente && Status != StatusTransferencia.Processando)
                throw new InvalidOperationException("Apenas transferências pendentes ou em processamento podem ser concluídas");
            
            Status = StatusTransferencia.Concluida;
            DataProcessamento = DateTime.UtcNow;
            Taxa = taxa;
            
            // Atualizar saldos das contas
            ContaOrigem.Debitar(Valor + (taxa ?? 0));
            ContaDestino.Creditar(Valor);
        }

        public void Cancelar(string motivo)
        {
            Status = StatusTransferencia.Cancelada;
            Descricao = $"{Descricao} [CANCELADA: {motivo}]";
        }

        public void Falhar(string motivo)
        {
            Status = StatusTransferencia.Falha;
            Descricao = $"{Descricao} [FALHA: {motivo}]";
        }

        public void IniciarProcessamento()
        {
            if (Status != StatusTransferencia.Pendente)
                throw new InvalidOperationException("Apenas transferências pendentes podem iniciar processamento");
            
            Status = StatusTransferencia.Processando;
        }

        public bool EhDiaUtil()
        {
            var data = DataSolicitacao;
            
            // Verifica se é fim de semana
            if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                return false;
            
            // Aqui você integraria com a API de feriados
            // Por enquanto, retorna true para dias de semana
            return true;
        }

        public decimal CalcularTaxa()
        {
            // Taxa de 1% para transferências acima de R$ 1000
            if (Valor > 1000)
                return Valor * 0.01m;
                
            // Taxa fixa de R$ 5 para finais de semana
            if (!EhDiaUtil())
                return 5.00m;
                
            return 0;
        }

        public string ObterResumo()
        {
            return $"Transferência: {ContaOrigem.NumeroConta} → {ContaDestino.NumeroConta} | R$ {Valor:N2} | {Status}";
        }

        private void Validar()
        {
            if (Valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(Valor));
            
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(Descricao));
            
            if (ContaOrigemId == ContaDestinoId)
                throw new ArgumentException("Conta origem e destino não podem ser iguais");
        }

        private string GerarCodigoRastreio()
        {
            var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"TRF{timestamp}{random}";
        }

        public override string ToString()
        {
            return $"Transferencia {CodigoRastreio} | R$ {Valor:N2} | {Status}";
        }
    }
}