// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;
using NUnit.Framework;
using Xtensive.Sql.Dom.Database.Comparer;
using Xtensive.Sql.Dom.Database.Providers;

namespace Xtensive.Sql.Dom.Tests.MsSql
{
  [TestFixture]
  public class ComparerTests
  {
    public const string ConnectionString1 = @"mssql2005://localhost/AdventureWorks";
    public const string ConnectionString2 = @"mssql2005://localhost/AdventureWorksDW"; 
    
    [Test]
    [Explicit]
    [Category("Debug")]
    public void CatalogCompare()
    {
      Database.Model model1 = GetModel(ConnectionString1);
      Database.Model model2 = GetModel(ConnectionString2);
      var result = new SqlComparer().Compare(model1.DefaultServer.DefaultCatalog, model2.DefaultServer.DefaultCatalog, null);
    }

    private Database.Model GetModel(string connectionString)
    {
      var provider = new SqlConnectionProvider();
      using (var connection = provider.CreateConnection(connectionString) as SqlConnection) {
        if (connection==null)
          throw new InvalidOperationException(string.Format("Unable to connect to {0}", connectionString));
        connection.Open();
        var modelProvider = new SqlModelProvider(connection);
        return Database.Model.Build(modelProvider);
      }
    }
  }
}