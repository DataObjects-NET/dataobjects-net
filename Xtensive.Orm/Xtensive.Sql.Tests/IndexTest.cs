// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests
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
      if (t!=null) {
        ExecuteNonQuery(SqlDdl.Drop(t));
        schema.Tables.Remove(t);
      }

      CreateTable();
    }

    protected override void TestFixtureTearDown()
    {
      Table t = schema.Tables[TableName];
      if (t!=null)
        ExecuteNonQuery(SqlDdl.Drop(t));
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
      i.CreateIndexColumn(t.TableColumns["first"]);
      i.CreateIndexColumn(t.TableColumns["second"]);
      var tr = SqlDml.TableRef(t);
      i.Where = SqlDml.IsNotNull(tr["first"]) && SqlDml.IsNotNull(tr["second"]);
      ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[TableName];
      var i2 = t2.Indexes[FilteredIndexName];
      Assert.IsNotNull(i2);
      Assert.AreEqual(2, i2.Columns.Count);
      Assert.IsTrue(!i2.Where.IsNullReference());
    }
  }
}