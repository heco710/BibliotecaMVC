-- Ejecutar con sqlcmd -v DatabaseName=BibliotecaDB o Setup-Database.ps1.
USE master;
GO
IF DB_ID(N'$(DatabaseName)') IS NULL
    EXEC(N'CREATE DATABASE [$(DatabaseName)]');
GO
USE [$(DatabaseName)];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias (
        ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(250) NULL
    );
    INSERT INTO dbo.Categorias (Nombre, Descripcion) VALUES
        (N'Novela', N'Obras narrativas de ficción'),
        (N'Ciencia Ficción', N'Obras relacionadas con ciencia y tecnología'),
        (N'Historia', N'Obras relacionadas con acontecimientos históricos');
END
ELSE IF (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Categorias')) <> 3
    OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Categorias') AND name = N'ID' AND system_type_id = 56 AND is_identity = 1 AND is_nullable = 0)
    OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Categorias') AND name = N'Nombre' AND system_type_id = 231 AND max_length = 200 AND is_nullable = 0)
    OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Categorias') AND name = N'Descripcion' AND system_type_id = 231 AND max_length = 500 AND is_nullable = 1)
    OR NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.Categorias') AND i.is_primary_key = 1
          AND c.name = N'ID' AND ic.key_ordinal = 1
          AND NOT EXISTS (SELECT 1 FROM sys.index_columns extra WHERE extra.object_id = i.object_id AND extra.index_id = i.index_id AND extra.key_ordinal > 1)
    )
BEGIN
    THROW 50001, 'dbo.Categorias tiene un esquema incompatible. No se modificaron sus datos.', 1;
END;
COMMIT;
