namespace BMPTec.Domain.Enums
{
    /// <summary>
    /// Tipos de conta bancária disponíveis no sistema
    /// </summary>
    public enum TipoConta
    {
        /// <summary>
        /// Conta Corrente - Para movimentações diárias
        /// </summary>
        Corrente = 1,
        
        /// <summary>
        /// Conta Poupança - Para economias com rendimento
        /// </summary>
        Poupanca = 2,
        
        /// <summary>
        /// Conta Salário - Para recebimento de salário
        /// </summary>
        Salario = 3,
        
        /// <summary>
        /// Conta Investimento - Para aplicações financeiras
        /// </summary>
        Investimento = 4,
        
        /// <summary>
        /// Conta Universitária - Para estudantes
        /// </summary>
        Universitaria = 5,
        
        /// <summary>
        /// Conta Conjunta - Para mais de um titular
        /// </summary>
        Conjunta = 6,
        
        /// <summary>
        /// Conta Digital - Exclusivamente online
        /// </summary>
        Digital = 7
    }
}