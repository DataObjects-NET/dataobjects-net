// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [TestFixture, Explicit]
  public class ExtractorTest : SqlTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void FullTextIndexExtractorTest()
    {
      var schema = Driver.ExtractDefaultSchema(Connection);
    }

    [Test]
    public void ExpressionIndexExtractorTest()
    {
      string guid = Guid.NewGuid().ToString();
      string tableName = "tbl" + guid;
      string indexName = "ix" + guid;
      string tableCreation = "CREATE TABLE \"" + tableName + "\"(col1 text, col2 text)";
      string indexCreation = "CREATE INDEX \"" + indexName + "\" ON \"" + tableName + "\"(col1,col2,(col1||col2))";

      Schema schema = null;
      try {
        Connection.BeginTransaction();

        using (var cmd = Connection.CreateCommand(tableCreation + ";" + indexCreation)) {
          cmd.ExecuteNonQuery();
        }
        schema = Driver.ExtractDefaultSchema(Connection);
      }
      finally {
        Connection.Rollback();
      }

      var table = schema.Tables[tableName];
      Assert.AreEqual(1, table.Indexes.Count);
      var index = table.Indexes[indexName];
      Assert.AreEqual(3, index.Columns.Count);
      Assert.AreSame(table.Columns[0], index.Columns[0].Column);
      Assert.AreSame(table.Columns[1], index.Columns[1].Column);
      Assert.IsNull(index.Columns[0].Expression);
      Assert.IsNull(index.Columns[1].Expression);
      Assert.IsNull(index.Columns[2].Column);
      Assert.IsNotNull(index.Columns[2].Expression);
    }
  }
}