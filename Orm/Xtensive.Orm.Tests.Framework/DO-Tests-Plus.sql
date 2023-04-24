--------------
-- DO-Tests-1
--------------

-- Dropping the database
USE master
GO
if exists (select * from sysdatabases where name='DO-Tests-1')
  drop database [DO-Tests-1]
GO

CREATE DATABASE [DO-Tests-1]
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


CREATE SCHEMA Model1
GO
CREATE SCHEMA Model2
GO
CREATE SCHEMA Model3
GO
CREATE SCHEMA Model4
GO
CREATE SCHEMA Model5
GO
CREATE SCHEMA Model6
GO
CREATE SCHEMA Model7
GO
CREATE SCHEMA Model8
GO
CREATE SCHEMA Model9
GO
CREATE SCHEMA Model10
GO
CREATE SCHEMA Model11
GO
CREATE SCHEMA Model12
GO

CREATE USER readonlydotest WITH PASSWORD = 'readonlydotest'
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

CREATE DATABASE [DO-Tests-2]
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

CREATE SCHEMA Model1
GO
CREATE SCHEMA Model2
GO
CREATE SCHEMA Model3
GO
CREATE SCHEMA Model4
GO
CREATE SCHEMA Model5
GO
CREATE SCHEMA Model6
GO
CREATE SCHEMA Model7
GO
CREATE SCHEMA Model8
GO
CREATE SCHEMA Model9
GO
CREATE SCHEMA Model10
GO
CREATE SCHEMA Model11
GO
CREATE SCHEMA Model12
GO

CREATE USER readonlydotest WITH PASSWORD = 'readonlydotest'
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


CREATE DATABASE [DO-Tests-3]
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

CREATE USER readonlydotest WITH PASSWORD = 'readonlydotest'
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

CREATE DATABASE [DO-Tests-4]
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

CREATE USER readonlydotest WITH PASSWORD = 'readonlydotest'
GO

