-- Criar banco se nÃ£o existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BMPTecDB_Dev')
BEGIN
    CREATE DATABASE BMPTecDB_Dev;
    PRINT 'Banco BMPTecDB_Dev criado.';
END
ELSE
BEGIN
    PRINT 'Banco BMPTecDB_Dev jÃ¡ existe.';
END
GO

-- Usar o banco
USE BMPTecDB_Dev;
GO

-- Habilitar READ_COMMITTED_SNAPSHOT (Ã³timo para EF Core)
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

-- Verificar criaÃ§Ã£o
PRINT '=== VERIFICAÃ‡ÃƒO FINAL ===';
SELECT name AS 'Sequence', start_value AS 'InÃ­cio', increment AS 'Incremento' 
FROM sys.sequences 
WHERE name LIKE 'Seq_%';
GO
