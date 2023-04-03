-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests')
  drop database [DO-Tests]
GO

-- Creating the database
CREATE DATABASE [DO-Tests]
GO

USE [DO-Tests]
GO

ALTER DATABASE [DO-Tests]
SET ALLOW_SNAPSHOT_ISOLATION ON

ALTER DATABASE [DO-Tests]
SET READ_COMMITTED_SNAPSHOT ON
GO

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
IN PATH N''' + @ftdata_directory + N'DO-Tests''
WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
AUTHORIZATION [dbo]')
GO
