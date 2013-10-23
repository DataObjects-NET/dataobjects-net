// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.SqlServer.v09
{
  [Serializable]
  public class IndexTest2005 : IndexTest
  {
    protected override void CreateTable()
    {
      Table t;
      t = schema.CreateTable(TableName);
      t.CreateColumn("first", new SqlValueType(SqlType.VarChar, 50));
      t.CreateColumn("second", new SqlValueType(SqlType.VarChar, 50));
      var c1 =t.CreateColumn("third", new SqlValueType(SqlType.VarChar));
      var c2 =t.CreateColumn("forth", new SqlValueType(SqlType.VarChar));

      var tr = SqlDml.TableRef(t);
      c1.Expression = SqlDml.Concat(tr["first"], " ", tr["second"]);
      c1.IsPersisted = false;
      c1.IsNullable = true;

      c2.Expression = SqlDml.Concat(tr["second"], " ", tr["first"]);
      c2.IsPersisted = false;
      c2.IsNullable = true;

      ExecuteNonQuery(SqlDdl.Create(t));
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2005);
    }

    [Test]
    public override void CreateExpressionIndexTest()
    {
      // Creating index
      var t = schema.Tables[TableName];
      var i = t.CreateIndex(ExpressionIndexName);
      i.CreateIndexColumn(t.TableColumns["third"]);
      i.CreateIndexColumn(t.TableColumns["forth"]);
      ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[TableName];
      var i2 = t2.Indexes[ExpressionIndexName];
      Assert.IsNotNull(i2);
      Assert.AreEqual(2, i2.Columns.Count);

      Assert.IsTrue(!t2.TableColumns["third"].Expression.IsNullReference());
      Assert.IsTrue(!t2.TableColumns["forth"].Expression.IsNullReference());
    }

    public override void CreateFilteredIndexTest()
    {
      if (GetType() == typeof(IndexTest2005))
        Assert.Ignore("Filtered indexes are not supported.");
    }
  }
}