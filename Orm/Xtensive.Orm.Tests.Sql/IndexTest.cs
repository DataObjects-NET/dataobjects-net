// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [Serializable]
  [TestFixture]
  public abstract class IndexTest : SqlTest
  {
    protected const string TableName = "index_table";
    protected const string ExpressionIndexName = "IX_EXPRESSION";
    protected const string FilteredIndexName = "IX_FILTERED";
    protected Schema schema;
    protected Catalog catalog;

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      catalog = ExtractCatalog();
      schema = catalog.DefaultSchema;
      Table t = schema.Tables[TableName];
      try {
        Connection.BeginTransaction();
        if (t != null) {
          _ = ExecuteNonQuery(SqlDdl.Drop(t));
          _ = schema.Tables.Remove(t);
        }

        CreateTable();
        Connection.Commit();
      }
      catch {
        if (Connection.ActiveTransaction != null)
          Connection.Rollback();
        Connection.Close();
        Connection.Dispose();
        throw;

      }
    }

    protected override void TestFixtureTearDown()
    {
      if (Connection.State != System.Data.ConnectionState.Open)
        base.TestFixtureSetUp();
      if (schema != null) {
        Table t = schema.Tables[TableName];
        if (t != null) {
          try {
            Connection.BeginTransaction();
            _ = ExecuteNonQuery(SqlDdl.Drop(t));
            Connection.Commit();
          }
          catch {
            if (Connection.ActiveTransaction != null)
              Connection.Rollback();
          }
        }
      }
      base.TestFixtureTearDown();
    }

    protected abstract void CreateTable();

    [Test]
    public abstract void CreateExpressionIndexTest();

    [Test]
    public virtual void CreateFilteredIndexTest()
    {
      // Creating index
      var t = schema.Tables[TableName];
      var i = t.CreateIndex(FilteredIndexName);
      _ = i.CreateIndexColumn(t.TableColumns["first"]);
      _ = i.CreateIndexColumn(t.TableColumns["second"]);
      var tr = SqlDml.TableRef(t);
      i.Where = SqlDml.IsNotNull(tr["first"]) && SqlDml.IsNotNull(tr["second"]);
      _ = ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[TableName];
      var i2 = t2.Indexes[FilteredIndexName];
      Assert.IsNotNull(i2);
      Assert.AreEqual(2, i2.Columns.Count);
      Assert.IsTrue(i2.Where is not null);
    }
  }
}