USE master;
GO

-- Criar banco se nÃ£o existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BMPTecDB_Dev')
    CREATE DATABASE BMPTecDB_Dev;
GO

USE BMPTecDB_Dev;
GO

-- Criar sequences
IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_CONTA')
    CREATE SEQUENCE Seq_CONTA START WITH 1000 INCREMENT BY 1;

IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_TRANSFERENCIA')
    CREATE SEQUENCE Seq_TRANSFERENCIA START WITH 1000 INCREMENT BY 1;

IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'Seq_CLIENTE')
    CREATE SEQUENCE Seq_CLIENTE START WITH 1000 INCREMENT BY 1;
GO

SELECT 'Sequences criadas com sucesso!' as Status;
SELECT name, start_value, increment FROM sys.sequences WHERE name LIKE 'Seq_%';
GO
