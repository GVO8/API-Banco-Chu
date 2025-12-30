# Script de setup para Docker - Versão Final
Write-Host "=== SETUP BMPTec API com Docker ===" -ForegroundColor Green

# 1. Limpar ambiente
Write-Host "`n1. Limpando ambiente anterior..." -ForegroundColor Yellow
docker-compose down -v 2>&1 | Out-Null

# 2. Criar init.sql básico
Write-Host "`n2. Criando init.sql..." -ForegroundColor Yellow
$initSql = @"
-- Criar banco de dados
CREATE DATABASE BMPTecDB_Dev;
GO

USE BMPTecDB_Dev;
GO

ALTER DATABASE BMPTecDB_Dev SET READ_COMMITTED_SNAPSHOT ON;
GO
"@

New-Item -ItemType Directory -Force -Path ".\scripts"
$initSql | Out-File -FilePath ".\scripts\init.sql" -Encoding UTF8 -Force
Write-Host "  init.sql criado." -ForegroundColor Green

# 3. Iniciar containers
Write-Host "`n3. Iniciando containers..." -ForegroundColor Yellow
docker-compose up -d 2>&1 | Out-Null

# 4. Aguardar SQL Server iniciar
Write-Host "`n4. Aguardando SQL Server (60 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

# 5. Verificar se SQL Server está rodando
Write-Host "`n5. Verificando status do SQL Server..." -ForegroundColor Yellow
docker logs bmptec-sqlserver --tail 5

# 6. Criar sequences via sqlcmd (com flag -C para SSL)
Write-Host "`n6. Criando banco e sequences..." -ForegroundColor Yellow
$createSql = @"
-- Criar banco se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BMPTecDB_Dev')
BEGIN
    CREATE DATABASE BMPTecDB_Dev;
    PRINT 'Banco BMPTecDB_Dev criado.';
END
ELSE
BEGIN
    PRINT 'Banco BMPTecDB_Dev já existe.';
END
GO

-- Usar o banco
USE BMPTecDB_Dev;
GO

-- Habilitar READ_COMMITTED_SNAPSHOT (ótimo para EF Core)
ALTER DATABASE BMPTecDB_Dev SET READ_COMMITTED_SNAPSHOT ON;
PRINT 'READ_COMMITTED_SNAPSHOT habilitado.';
GO

-- Criar sequences (IMPORTANTE para seu SequenceGenerator)
IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_CONTA')
BEGIN
    CREATE SEQUENCE Seq_CONTA START WITH 1000 INCREMENT BY 1;
    PRINT 'Sequence Seq_CONTA criada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_TRANSFERENCIA')
BEGIN
    CREATE SEQUENCE Seq_TRANSFERENCIA START WITH 1000 INCREMENT BY 1;
    PRINT 'Sequence Seq_TRANSFERENCIA criada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_CLIENTE')
BEGIN
    CREATE SEQUENCE Seq_CLIENTE START WITH 1000 INCREMENT BY 1;
    PRINT 'Sequence Seq_CLIENTE criada.';
END
GO

-- Verificar criação
PRINT '=== VERIFICAÇÃO FINAL ===';
SELECT name AS 'Sequence', start_value AS 'Início', increment AS 'Incremento' 
FROM sys.sequences 
WHERE name LIKE 'Seq_%';
GO
"@

# Salvar SQL em arquivo
$createSql | Out-File -FilePath ".\scripts\setup_database.sql" -Encoding UTF8 -Force

# Copiar para container
docker cp ".\scripts\setup_database.sql" bmptec-sqlserver:/tmp/setup_database.sql 2>&1 | Out-Null

# Encontrar sqlcmd
Write-Host "  Localizando sqlcmd..." -ForegroundColor Gray
$sqlcmdPath = docker exec bmptec-sqlserver find /opt -name "sqlcmd" -type f 2>/dev/null | Select-Object -First 1

if ($sqlcmdPath) {
    Write-Host "  sqlcmd encontrado em: $sqlcmdPath" -ForegroundColor Green
    
    # Executar SQL confiando no certificado (-C)
    Write-Host "  Executando script de criação..." -ForegroundColor Gray
    try {
        $result = docker exec bmptec-sqlserver $sqlcmdPath -S localhost -U SA -P "YourStrong@Passw0rd" -C -i /tmp/setup_database.sql
        Write-Host "  Resultado:" -ForegroundColor Green
        $result
    } catch {
        Write-Host "  Tentando sem encriptação..." -ForegroundColor Yellow
        # Tentar com -N (sem encriptação)
        $result = docker exec bmptec-sqlserver $sqlcmdPath -S localhost -U SA -P "YourStrong@Passw0rd" -N -i /tmp/setup_database.sql
        Write-Host "  Resultado:" -ForegroundColor Green
        $result
    }
} else {
    Write-Host "  sqlcmd não encontrado." -ForegroundColor Red
}

# 7. Instalar e configurar Entity Framework
Write-Host "`n7. Configurando Entity Framework..." -ForegroundColor Yellow

# Instalar/atualizar dotnet-ef
try {
    Write-Host "  Atualizando dotnet-ef..." -ForegroundColor Gray
    dotnet tool update --global dotnet-ef --version 8.* 2>&1 | Out-Null
    Write-Host "  dotnet-ef atualizado." -ForegroundColor Green
} catch {
    Write-Host "  Instalando dotnet-ef..." -ForegroundColor Gray
    dotnet tool install --global dotnet-ef --version 8.* 2>&1 | Out-Null
    Write-Host "  dotnet-ef instalado." -ForegroundColor Green
}

# Restaurar pacotes
Write-Host "  Restaurando pacotes NuGet..." -ForegroundColor Gray
dotnet restore 2>&1 | Out-Null

# Aplicar migrations
Write-Host "  Aplicando migrations do banco..." -ForegroundColor Gray
try {
    # Primeiro tenta atualizar se já existir migration
    dotnet ef database update --project BMPTec.Infrastructure --startup-project BMPTec.Api 2>&1 | Out-Null
    Write-Host "  Migrations aplicadas com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "  Criando nova migration..." -ForegroundColor Yellow
    try {
        dotnet ef migrations add InitialCreate --project BMPTec.Infrastructure --startup-project BMPTec.Api --output-dir Data/Migrations 2>&1 | Out-Null
        dotnet ef database update --project BMPTec.Infrastructure --startup-project BMPTec.Api 2>&1 | Out-Null
        Write-Host "  Migration criada e aplicada!" -ForegroundColor Green
    } catch {
        Write-Host "  Erro ao criar/applicar migrations: $_" -ForegroundColor Red
    }
}

# 8. Verificação final
Write-Host "`n8. Verificação final..." -ForegroundColor Yellow
Write-Host "`nPara testar manualmente:" -ForegroundColor White
Write-Host "1. Abra HeidiSQL" -ForegroundColor Cyan
Write-Host "2. Conecte com:" -ForegroundColor Cyan
Write-Host "   - Host: localhost" -ForegroundColor White
Write-Host "   - Porta: 1433" -ForegroundColor White
Write-Host "   - Usuário: SA" -ForegroundColor White
Write-Host "   - Senha: YourStrong@Passw0rd" -ForegroundColor White
Write-Host "   - Database: BMPTecDB_Dev" -ForegroundColor White
Write-Host "`n3. Execute para verificar:" -ForegroundColor Cyan
Write-Host @"
   -- Verificar sequences
   SELECT * FROM sys.sequences;
   
   -- Verificar tabelas criadas pelo EF Core
   SELECT * FROM sys.tables ORDER BY name;
   
   -- Testar sequences
   SELECT NEXT VALUE FOR Seq_CONTA;
   SELECT NEXT VALUE FOR Seq_TRANSFERENCIA;
"@ -ForegroundColor Green

Write-Host "`n=== SETUP CONCLUÍDO! ===" -ForegroundColor Green
Write-Host "`nSe encontrar problemas:" -ForegroundColor Yellow
Write-Host "1. Sequences não criadas? Execute manualmente no HeidiSQL:" -ForegroundColor White
Write-Host @"
   USE BMPTecDB_Dev;
   GO
   CREATE SEQUENCE Seq_CONTA START WITH 1000 INCREMENT BY 1;
   CREATE SEQUENCE Seq_TRANSFERENCIA START WITH 1000 INCREMENT BY 1;
   CREATE SEQUENCE Seq_CLIENTE START WITH 1000 INCREMENT BY 1;
"@ -ForegroundColor Cyan

Write-Host "`n2. Para reiniciar do zero:" -ForegroundColor White
Write-Host "   docker-compose down -v" -ForegroundColor Cyan
Write-Host "   .\scripts\setup-docker.ps1" -ForegroundColor Cyan