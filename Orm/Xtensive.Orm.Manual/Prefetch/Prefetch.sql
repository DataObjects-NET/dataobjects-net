-- Batch 1
SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] FROM [dbo].[Person] [a];

-- Batch 2
exec sp_executesql N'
SELECT [a].[Id], [a].[TypeId], [a].[Photo] FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Photo], [b].[Manager.Id] 
  FROM [dbo].[Person] [b]) [a] 
WHERE [a].[Id] IN (@p1_0_0_0, @p1_0_1_0);

SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] FROM [dbo].[Person] [a] 
WHERE ([a].[Manager.Id] = @p2_0);

SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] FROM [dbo].[Person] [a] 
WHERE ([a].[Manager.Id] = @p3_0);

',N'@p1_0_0_0 int,@p1_0_1_0 int,@p2_0 int,@p3_0 int',@p1_0_0_0=1,@p1_0_1_0=2,@p2_0=1,@p3_0=2


-- Batch 1
SELECT [a].[Id], 101 AS [TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] FROM [dbo].[Person] [a];

-- Batch 2
exec sp_executesql N'
SELECT [a].[Id], [a].[TypeId], [a].[Photo] FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Photo], [b].[Manager.Id] 
  FROM [dbo].[Person] [b]) [a] 
WHERE [a].[Id] IN (@p1_0_0_0, @p1_0_1_0);

SELECT TOP 40 [a].[Id], [a].[TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] 
FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Manager.Id] 
  FROM [dbo].[Person] [b] 
  WHERE ([b].[Manager.Id] = @p2_0)) [a] 
ORDER BY [a].[Id] ASC;

SELECT TOP 40 [a].[Id], [a].[TypeId], [a].[Name], [a].[BirthDay], [a].[Manager.Id] 
FROM (
  SELECT [b].[Id], 101 AS [TypeId], [b].[Name], [b].[BirthDay], [b].[Manager.Id] 
  FROM [dbo].[Person] [b] 
  WHERE ([b].[Manager.Id] = @p3_0)) [a] 
ORDER BY [a].[Id] ASC;

',N'@p1_0_0_0 int,@p1_0_1_0 int,@p2_0 int,@p3_0 int',@p1_0_0_0=1,@p1_0_1_0=2,@p2_0=1,@p3_0=2