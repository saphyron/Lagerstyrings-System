/* ===== Bootstrap: DB + Login + User + Minimal Permissions ===== */
/* Run this in the master database with sufficient privileges. */

DECLARE @DbName    sysname       = N'LSSDb';
DECLARE @Login     sysname       = N'LSSUser';
DECLARE @Password  nvarchar(128) = N'LSS-P@ssw0rd';

/* 1) Create server login if missing (PASSWORD must be literal → dynamic SQL) */
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @Login)
BEGIN
    DECLARE @sql_login nvarchar(max) =
        N'CREATE LOGIN [' + @Login + N'] WITH PASSWORD = N''' +
        REPLACE(@Password, '''', '''''') + N''', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;';
    EXEC (@sql_login);
END

/* 2) Create database if missing */
IF DB_ID(@DbName) IS NULL
BEGIN
    DECLARE @sql_db nvarchar(max) = N'CREATE DATABASE [' + @DbName + N'];';
    EXEC (@sql_db);
END

/* 3) All database-scoped actions in one dynamic block (context is inside the block) */
DECLARE @sql_db_actions nvarchar(max) = N'
USE [' + @DbName + N'];

/* Ensure user exists and is mapped to the server login */
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N''' + @Login + N''')
BEGIN
    CREATE USER [' + @Login + N'] FOR LOGIN [' + @Login + N'] WITH DEFAULT_SCHEMA = [dbo];
END
ELSE
BEGIN
    /* If user exists but is not mapped (or mapped to wrong login), remap it */
    BEGIN TRY
        ALTER USER [' + @Login + N'] WITH LOGIN = [' + @Login + N'];
    END TRY
    BEGIN CATCH
        /* If ALTER fails because it is already mapped, ignore */
    END CATCH;
    ALTER USER [' + @Login + N'] WITH DEFAULT_SCHEMA = [dbo];
END

/* Grant minimal runtime permissions */
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON r.principal_id = drm.role_principal_id
    JOIN sys.database_principals m ON m.principal_id = drm.member_principal_id
    WHERE r.name = N''db_datareader'' AND m.name = N''' + @Login + N'''
)
    ALTER ROLE [db_datareader] ADD MEMBER [' + @Login + N'];

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON r.principal_id = drm.role_principal_id
    JOIN sys.database_principals m ON m.principal_id = drm.member_principal_id
    WHERE r.name = N''db_datawriter'' AND m.name = N''' + @Login + N'''
)
    ALTER ROLE [db_datawriter] ADD MEMBER [' + @Login + N'];

/* Return a quick proof that we are in the right DB and the user exists */
SELECT
    DbContext = DB_NAME(),
    UserName  = p.name,
    p.type_desc,
    p.authentication_type_desc
FROM sys.database_principals p
WHERE p.name = N''' + @Login + N''';
';

EXEC (@sql_db_actions);
GO


-- Verification script

-- Server-level login?
SELECT name, type_desc FROM sys.server_principals WHERE name = N'LSSUser';

-- Database-level user?
USE [LSSDb];
SELECT name, type_desc, authentication_type_desc
FROM sys.database_principals
WHERE name = N'LSSUser';

-- Role memberships?
SELECT r.name AS role_name, m.name AS member_name
FROM sys.database_role_members drm
JOIN sys.database_principals r ON r.principal_id = drm.role_principal_id
JOIN sys.database_principals m ON m.principal_id = drm.member_principal_id
WHERE m.name = N'LSSUser';
