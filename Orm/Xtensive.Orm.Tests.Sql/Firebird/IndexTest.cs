// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.24

using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
  public abstract class IndexTest : Sql.IndexTest
  {
    public override void SetUp()
    {
      TestHelpers.StartTraceToLogFile(this);
      base.SetUp();
      // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
      RealTestFixtureSetUp();
    }

    public override void TearDown()
    {
      base.TearDown();
      // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
      RealTestFixtureTearDown();
      TestHelpers.StopTraceToLogFile(this);
    }

    protected override void CreateTable()
    {
      Table t;
      t = schema.CreateTable(TableName);
      t.CreateColumn("first", new SqlValueType(SqlType.VarChar, 50));
      t.CreateColumn("second", new SqlValueType(SqlType.VarChar, 50));
      ExecuteNonQuery(SqlDdl.Create(t));
    }

    [Test]
    public override void CreateExpressionIndexTest()
    {
      // Creating index
      var t = schema.Tables[TableName];
      var i = t.CreateIndex(ExpressionIndexName);
      var tr = SqlDml.TableRef(t);
      i.CreateIndexColumn(SqlDml.Concat(tr["first"], " ", tr["second"]));
      ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[TableName];
      var i2 = t2.Indexes[ExpressionIndexName];
      Assert.IsNotNull(i2);
      Assert.AreEqual(1, i2.Columns.Count);

      Assert.IsTrue(!i2.Columns[0].Expression.IsNullReference());
    }

    [Test, Ignore("Test is not implemented")]
    public override void CreateFilteredIndexTest()
    {
    }
  }
}

