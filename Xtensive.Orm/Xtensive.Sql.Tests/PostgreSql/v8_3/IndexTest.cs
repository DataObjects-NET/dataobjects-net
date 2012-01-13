// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.PostgreSql.v8_3
{
  public class IndexTest : Tests.IndexTest
  {
    protected override string Url
    {
      get { return TestUrl.PostgreSql83; }
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
      i.CreateIndexColumn(tr["first"]);
      i.CreateIndexColumn(tr["second"]);
      i.CreateIndexColumn(SqlDml.Concat(tr["first"], " ", tr["second"]));
      i.CreateIndexColumn(SqlDml.Concat(tr["second"], " ", tr["first"]));
      ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[TableName];
      var i2 = t2.Indexes[ExpressionIndexName];
      Assert.IsNotNull(i2);
      Assert.AreEqual(4, i2.Columns.Count);

      Assert.IsTrue(!i2.Columns[2].Expression.IsNullReference());
      Assert.IsTrue(!i2.Columns[3].Expression.IsNullReference());
    }

  }
}