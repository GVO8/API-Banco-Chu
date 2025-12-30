using BMPTec.Domain.Entities.Base;
using BMPTec.Domain.Enums;

namespace BMPTec.Domain.Entities
{
    public class Conta : AuditableEntity
    {
        // Construtor protegido para EF
        protected Conta() { }

        // Construtor principal
        public Conta(
            string numeroConta,
            string agencia,
            TipoConta tipoConta,
            Cliente cliente,
            decimal saldoInicial = 0)
        {
            NumeroConta = numeroConta ?? throw new ArgumentNullException(nameof(numeroConta));
            Agencia = agencia ?? throw new ArgumentNullException(nameof(agencia));
            TipoConta = tipoConta;
            Cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            Saldo = saldoInicial;
            Status = StatusConta.Ativa;
            DataAbertura = DateTime.UtcNow;
            
            Validate();
        }

        public string NumeroConta { get; private set; }
        public string Agencia { get; private set; } = "0001"; // Agência padrão
        public TipoConta TipoConta { get; private set; }
        public decimal Saldo { get; private set; }
        public StatusConta Status { get; private set; }
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataEncerramento { get; private set; }
        
        // Relacionamentos
        public Guid ClienteId { get; private set; }
        public virtual Cliente Cliente { get; private set; }
        public virtual ICollection<Transferencia> Transferencias { get; private set; } = new List<Transferencia>();

        // Comportamentos do domínio
        public void Creditar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(valor));
            
            if (Status != StatusConta.Ativa)
                throw new InvalidOperationException("Conta inativa não pode receber créditos");
                
            Saldo += valor;
        }

        public void Debitar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(valor));
            
            if (Status != StatusConta.Ativa)
                throw new InvalidOperationException("Conta inativa não pode realizar débitos");
                
            if (Saldo < valor)
                throw new InvalidOperationException("Saldo insuficiente");
                
            Saldo -= valor;
        }

        public void Encerrar()
        {
            if (Status == StatusConta.Encerrada)
                throw new InvalidOperationException("Conta já está encerrada");
                
            if (Saldo > 0)
                throw new InvalidOperationException("Não é possível encerrar conta com saldo positivo");
                
            Status = StatusConta.Encerrada;
            DataEncerramento = DateTime.UtcNow;
        }

        public void Bloquear()
        {
            Status = StatusConta.Bloqueada;
        }

        public void Desbloquear()
        {
            if (Status == StatusConta.Bloqueada)
                Status = StatusConta.Ativa;
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(NumeroConta))
                throw new ArgumentException("Número da conta é obrigatório");
                
            if (Cliente == null)
                throw new ArgumentException("Cliente é obrigatório");
        }

        public override string ToString()
        {
            return $"Número Conta: {NumeroConta} - Agência: {Agencia}";
        }
    }
}