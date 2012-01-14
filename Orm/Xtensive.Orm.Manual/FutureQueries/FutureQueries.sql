-- Batch 1
exec sp_executesql N'
INSERT INTO [dbo].[Person] ([Id], [Name], [BirthDay], [Manager.Id]) 
VALUES (@p1_0, @p1_1, @p1_2, @p1_3);

INSERT INTO [dbo].[Person] ([Id], [Name], [BirthDay], [Manager.Id]) 
VALUES (@p2_0, @p2_1, @p2_2, @p2_3);

SELECT COUNT_BIG(*) AS [column] 
FROM (
  SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] 
  FROM [dbo].[Person] [a] 
  WHERE ([a].[Manager.Id] IS NOT NULL)) [b];
  
SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] 
FROM [dbo].[Person] [a] 
WHERE ((SELECT COUNT_BIG(*) FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Manager.Id] 
  FROM [dbo].[Person] [b] 
  WHERE ([b].[Manager.Id] = [a].[Id])) [c]) <> 0);
  
SELECT [a].[Id], [a].[TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] 
FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Manager.Id] 
  FROM [dbo].[Person] [b]) [a] 
ORDER BY [a].[Name] ASC;
  
',N'
@p1_0 int,@p1_1 nvarchar(7),@p1_2 datetime,@p1_3 int,
@p2_0 int,@p2_1 nvarchar(8),@p2_2 datetime,@p2_3 int',
@p1_0=2,@p1_1=N'Manager',@p1_2='1753-01-01 00:00:00:000',@p1_3=NULL,
@p2_0=1,@p2_1=N'Employee',@p2_2='1753-01-01 00:00:00:000',@p2_3=2
