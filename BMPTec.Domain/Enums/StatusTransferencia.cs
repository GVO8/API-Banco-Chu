namespace BMPTec.Domain.Enums
{
    /// <summary>
    /// Status possíveis de uma transação bancária
    /// </summary>
    public enum StatusTransferencia
    {
        /// <summary>
        /// Transação concluída com sucesso
        /// </summary>
        Concluida = 1,
        
        /// <summary>
        /// Transação pendente de processamento
        /// </summary>
        Pendente = 2,
        
        /// <summary>
        /// Transação em processamento
        /// </summary>
        Processando = 3,
        
        /// <summary>
        /// Transação cancelada
        /// </summary>
        Cancelada = 4,
        
        /// <summary>
        /// Transação falhou
        /// </summary>
        Falha = 5
    }
}