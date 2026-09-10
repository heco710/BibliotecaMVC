-- Setup-Database.ps1 proporciona @Password como parámetro; nunca incluir claves aquí.
-- Ejecutar en la base de datos de la aplicación con una identidad administradora.
IF SUSER_ID(N'biblioteca_user') IS NULL
BEGIN
    IF @Password IS NULL THROW 50002, 'Se requiere una contraseña para crear el login.', 1;
    DECLARE @sql NVARCHAR(MAX) = N'CREATE LOGIN [biblioteca_user] WITH PASSWORD = ' + QUOTENAME(@Password, '''') + N', CHECK_POLICY = ON;';
    EXEC(@sql);
END;
IF USER_ID(N'biblioteca_user') IS NULL
    CREATE USER [biblioteca_user] FOR LOGIN [biblioteca_user];
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'biblioteca_user' AND sid <> SUSER_SID(N'biblioteca_user'))
    THROW 50003, 'El usuario existente pertenece a otro login. Revisar manualmente.', 1;
IF IS_ROLEMEMBER(N'db_datareader', N'biblioteca_user') <> 1
    ALTER ROLE db_datareader ADD MEMBER biblioteca_user;
IF IS_ROLEMEMBER(N'db_datawriter', N'biblioteca_user') <> 1
    ALTER ROLE db_datawriter ADD MEMBER biblioteca_user;
