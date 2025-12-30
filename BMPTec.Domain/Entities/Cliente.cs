using BMPTec.Domain.Entities.Base;

namespace BMPTec.Domain.Entities
{
    public class Cliente : AuditableEntity
    {
        protected Cliente() { }
        
        public Cliente(string nome, string cpf, string email, DateTime dataNascimento, string telefone)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            CPF = cpf ?? throw new ArgumentNullException(nameof(cpf));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            DataNascimento = dataNascimento;
            Telefone = telefone ?? throw new ArgumentNullException(nameof(telefone));
            Ativo = true;
            
            Validate();
        }

        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public string Email { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public string Telefone { get; private set; }
        public bool Ativo { get; private set; }
        
        // Relacionamentos
        public virtual ICollection<Conta> Contas { get; private set; } = new List<Conta>();

        public void Ativar() => Ativo = true;
        public void Inativar() => Ativo = false;
        
        public void AtualizarContato(string email, string telefone)
        {
            if (!string.IsNullOrWhiteSpace(email))
                Email = email;
                
            if (!string.IsNullOrWhiteSpace(telefone))
                Telefone = telefone;
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Nome) || Nome.Length < 3)
                throw new ArgumentException("Nome deve ter pelo menos 3 caracteres");
                
            var idade = DateTime.UtcNow.Year - DataNascimento.Year;
            if (DataNascimento.Date > DateTime.UtcNow.AddYears(-idade))
                idade--;
                
            if (idade < 18)
                throw new ArgumentException("Cliente deve ser maior de 18 anos");
        }
    }
}