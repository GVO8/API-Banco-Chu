namespace BMPTec.Domain.Enums
{
    /// <summary>
    /// Status possíveis de uma conta bancária
    /// </summary>
    public enum StatusConta
    {
        /// <summary>
        /// Conta ativa e operante
        /// </summary>
        Ativa = 1,
        
        /// <summary>
        /// Conta inativa (sem movimentação)
        /// </summary>
        Inativa = 2,
        
        /// <summary>
        /// Conta bloqueada por suspeita
        /// </summary>
        Bloqueada = 3,
        
        /// <summary>
        /// Conta encerrada definitivamente
        /// </summary>
        Encerrada = 4,
        
        /// <summary>
        /// Conta pendente de aprovação
        /// </summary>
        PendenteAprovacao = 5,
        
        /// <summary>
        /// Conta com restrições parciais
        /// </summary>
        Restrita = 6
    }
}