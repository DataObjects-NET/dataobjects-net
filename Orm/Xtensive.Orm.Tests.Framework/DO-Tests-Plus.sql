--------------
-- DO-Tests-1
--------------

-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests-1')
  drop database [DO-Tests-1]
GO

-- Creating the database
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @data_directory NVARCHAR(520)
SELECT @data_directory = @server_directory + N'DATA\'

EXECUTE (N'CREATE DATABASE [DO-Tests-1]
  ON PRIMARY (NAME = N''DO-Tests-1'', FILENAME = N''' + @data_directory + N'DO-Tests-1.mdf'')
  LOG ON (NAME = N''DO-Tests-1_log'',  FILENAME = N''' + @data_directory + N'DO-Tests-1.ldf'')')
GO

USE [DO-Tests-1]
GO

ALTER DATABASE [DO-Tests-1]
SET ALLOW_SNAPSHOT_ISOLATION ON

ALTER DATABASE [DO-Tests-1]
SET READ_COMMITTED_SNAPSHOT ON

-- Enabling full-text indexing there
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbo].[sp_fulltext_database] @action = 'enable'
end
GO

-- And creating full-text catalogue
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @ftdata_directory NVARCHAR(520)
SELECT @ftdata_directory = @server_directory + N'FTData\'

EXECUTE (N'CREATE FULLTEXT CATALOG [Default]
IN PATH N''' + @ftdata_directory + N'DO-Tests-1''
WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
AUTHORIZATION [dbo]')
GO

--------------
-- DO-Tests-2
--------------

-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests-2')
  drop database [DO-Tests-2]
GO

-- Creating the database
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @data_directory NVARCHAR(520)
SELECT @data_directory = @server_directory + N'DATA\'

EXECUTE (N'CREATE DATABASE [DO-Tests-2]
  ON PRIMARY (NAME = N''DO-Tests-2'', FILENAME = N''' + @data_directory + N'DO-Tests-2.mdf'')
  LOG ON (NAME = N''DO-Tests-2_log'',  FILENAME = N''' + @data_directory + N'DO-Tests-2.ldf'')')
GO

USE [DO-Tests-2]
GO

ALTER DATABASE [DO-Tests-2]
SET ALLOW_SNAPSHOT_ISOLATION ON

ALTER DATABASE [DO-Tests-2]
SET READ_COMMITTED_SNAPSHOT ON

-- Enabling full-text indexing there
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbo].[sp_fulltext_database] @action = 'enable'
end
GO

-- And creating full-text catalogue
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @ftdata_directory NVARCHAR(520)
SELECT @ftdata_directory = @server_directory + N'FTData\'

EXECUTE (N'CREATE FULLTEXT CATALOG [Default]
IN PATH N''' + @ftdata_directory + N'DO-Tests-2''
WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
AUTHORIZATION [dbo]')
GO

--------------
-- DO-Tests-3
--------------

-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests-3')
  drop database [DO-Tests-3]
GO

-- Creating the database
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @data_directory NVARCHAR(520)
SELECT @data_directory = @server_directory + N'DATA\'

EXECUTE (N'CREATE DATABASE [DO-Tests-3]
  ON PRIMARY (NAME = N''DO-Tests-3'', FILENAME = N''' + @data_directory + N'DO-Tests-3.mdf'')
  LOG ON (NAME = N''DO-Tests-3_log'',  FILENAME = N''' + @data_directory + N'DO-Tests-3.ldf'')')
GO

USE [DO-Tests-3]
GO

ALTER DATABASE [DO-Tests-3]
SET ALLOW_SNAPSHOT_ISOLATION ON

ALTER DATABASE [DO-Tests-3]
SET READ_COMMITTED_SNAPSHOT ON

-- Enabling full-text indexing there
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbo].[sp_fulltext_database] @action = 'enable'
end
GO

-- And creating full-text catalogue
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @ftdata_directory NVARCHAR(520)
SELECT @ftdata_directory = @server_directory + N'FTData\'

EXECUTE (N'CREATE FULLTEXT CATALOG [Default]
IN PATH N''' + @ftdata_directory + N'DO-Tests-3''
WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
AUTHORIZATION [dbo]')
GO

--------------
-- DO-Tests-4
--------------

-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests-4')
  drop database [DO-Tests-4]
GO

-- Creating the database
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @data_directory NVARCHAR(520)
SELECT @data_directory = @server_directory + N'DATA\'

EXECUTE (N'CREATE DATABASE [DO-Tests-4]
  ON PRIMARY (NAME = N''DO-Tests-4'', FILENAME = N''' + @data_directory + N'DO-Tests-4.mdf'')
  LOG ON (NAME = N''DO-Tests-4_log'',  FILENAME = N''' + @data_directory + N'DO-Tests-4.ldf'')')
GO

USE [DO-Tests-4]
GO

ALTER DATABASE [DO-Tests-4]
SET ALLOW_SNAPSHOT_ISOLATION ON

ALTER DATABASE [DO-Tests-4]
SET READ_COMMITTED_SNAPSHOT ON

-- Enabling full-text indexing there
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbo].[sp_fulltext_database] @action = 'enable'
end
GO

-- And creating full-text catalogue
DECLARE @server_directory NVARCHAR(520)
SELECT @server_directory = SUBSTRING(filename, 1, CHARINDEX(N'DATA\master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

DECLARE @ftdata_directory NVARCHAR(520)
SELECT @ftdata_directory = @server_directory + N'FTData\'

EXECUTE (N'CREATE FULLTEXT CATALOG [Default]
IN PATH N''' + @ftdata_directory + N'DO-Tests-4''
WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
AUTHORIZATION [dbo]')
GO
