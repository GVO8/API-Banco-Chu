using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BMPTec.Application.Interfaces.Services;
using BMPTec.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace BMPTec.Infrastructure.Services
{
    /// <summary>
    /// Implementação de SequenceGenerator usando banco de dados SQL Server
    /// </summary>
    public class DatabaseSequenceGenerator : ISequenceGenerator
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache  _cache;
        private readonly ILogger<DatabaseSequenceGenerator> _logger;
        private readonly object _lock = new object();
        
        // Cache de sequências em memória para performance
        private readonly Dictionary<string, long> _sequenceCache = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public DatabaseSequenceGenerator(
            AppDbContext context,
            IMemoryCache  cache,
            ILogger<DatabaseSequenceGenerator> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<long> NextAsync(string sequenceName)
        {
            await _semaphore.WaitAsync();
            
            try
            {
                // Tentar obter do cache Redis primeiro
                var cacheKey = $"sequence:{sequenceName}";
                _cache.TryGetValue(cacheKey, out string cachedValue);
                
                if (!string.IsNullOrEmpty(cachedValue) && long.TryParse(cachedValue, out var cachedSeq))
                {
                    var nextValue = cachedSeq + 1;
                    _cache.Set(cacheKey, nextValue.ToString());
                    
                    return nextValue;
                }

                // Se não tem cache, usar o banco de dados
                return await GetNextFromDatabaseAsync(sequenceName);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> GerarNumeroContaAsync()
        {
            // Obter próximo número da sequência
            var nextNumber = await NextAsync("CONTA");
            
            // Formatar como 6 dígitos
            var numeroBase = nextNumber.ToString("D6");
            
            // Gerar dígito verificador
            var digitoVerificador = GerarDigitoVerificador(numeroBase);
            
            // Formatar: 000001-8
            return $"{numeroBase}-{digitoVerificador}";
        }

        public async Task<string> GerarCodigoTransferenciaAsync()
        {
            var nextNumber = await NextAsync("TRANSFERENCIA");
            
            // Formato: CHU-20241227-000001
            var data = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"CHU-{data}-{nextNumber:D6}";
        }

        public string GerarNumeroAgencia()
        {
            // Para este exemplo, usamos agência fixa 0001
            // Em um sistema real, poderia vir de configuração ou sequência
            return "0001";
        }

        public int GerarDigitoVerificador(string numeroBase)
        {
            if (string.IsNullOrEmpty(numeroBase))
                throw new ArgumentException("Número base não pode ser vazio", nameof(numeroBase));

            // Algoritmo módulo 11 para dígito verificador
            int soma = 0;
            int peso = 2;
            
            // Percorrer o número da direita para esquerda
            for (int i = numeroBase.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(numeroBase[i]))
                    throw new ArgumentException("Número base deve conter apenas dígitos", nameof(numeroBase));
                    
                int digito = numeroBase[i] - '0';
                soma += digito * peso;
                peso = peso == 9 ? 2 : peso + 1;
            }

            int resto = soma % 11;
            int digitoVerificador = 11 - resto;
            
            // Se digito for 10 ou 11, ajustar
            return digitoVerificador switch
            {
                10 => 0,
                11 => 1,
                _ => digitoVerificador
            };
        }

        public async Task ResetAsync(string sequenceName)
        {
            await _semaphore.WaitAsync();
            
            try
            {
                // Limpar cache
                var cacheKey = $"sequence:{sequenceName}";
                _cache.Remove(cacheKey);
                
                // Remover do dicionário local
                lock (_lock)
                {
                    _sequenceCache.Remove(sequenceName);
                }
                
                // Resetar no banco (se houver tabela de sequências)
                await ResetDatabaseSequenceAsync(sequenceName);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<long> GetNextFromDatabaseAsync(string sequenceName)
        {
            try
            {
                // Método 1: Usando tabela de sequências
                return await GetNextFromSequenceTableAsync(sequenceName);
            }
            catch (Exception ex)
            {
                // Método 2: Fallback usando MAX() da tabela
                return await GetNextFromMaxIdAsync(sequenceName);
            }
        }

         public async Task<long> GetNextFromSequenceTableAsync(string sequenceName)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT NEXT VALUE FOR Seq_{sequenceName}";
            
            await _context.Database.OpenConnectionAsync();
            var nextValue = await command.ExecuteScalarAsync();
            await _context.Database.CloseConnectionAsync();

            var cacheKey = $"sequence:{sequenceName}";
            _cache.Set(cacheKey, nextValue.ToString());
            
            return Convert.ToInt64(nextValue);
        }

        private async Task<long> GetNextFromMaxIdAsync(string sequenceName)
        {
            // Fallback: obter máximo ID da tabela relacionada
            long maxId = sequenceName switch
            {
                "CONTA" => await _context.Conta.MaxAsync(c => (long?)c.Id.GetHashCode()) ?? 0,
                "TRANSFERENCIA" => await _context.Transferencia.MaxAsync(t => (long?)t.Id.GetHashCode()) ?? 0,
                //"TRANSACAO" => await _context.Transacoes.MaxAsync(t => (long?)t.Id.GetHashCode()) ?? 0,
                _ => 0
            };
            
            return maxId + 1;
        }

        private async Task ResetDatabaseSequenceAsync(string sequenceName)
        {
            try
            {
                // Resetar sequence no SQL Server
                var sql = $"ALTER SEQUENCE Seq_{sequenceName} RESTART WITH 1000";
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível resetar a sequência {SequenceName}", sequenceName);
            }
        }
    }
}