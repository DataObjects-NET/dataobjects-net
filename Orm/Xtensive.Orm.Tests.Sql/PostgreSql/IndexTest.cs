// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public class IndexTest : Sql.IndexTest
  {
    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void CreateExpressionIndexTable(Schema schema)
    {
      var t = schema.CreateTable(ExpressionIndexTableName);
      _ = t.CreateColumn("first", new SqlValueType(SqlType.VarChar, 50));
      _ = t.CreateColumn("second", new SqlValueType(SqlType.VarChar, 50));
      _ = ExecuteNonQuery(SqlDdl.Create(t));
    }

    [Test]
    public override void CreateExpressionIndexTest()
    {
      base.CreateExpressionIndexTest();

      // Creating index
      var t = ExpressionIndexTable;
      var i = t.CreateIndex(ExpressionIndexName);
      var tr = SqlDml.TableRef(t);
      _ = i.CreateIndexColumn(tr["first"]);
      _ = i.CreateIndexColumn(tr["second"]);
      _ = i.CreateIndexColumn(SqlDml.Concat(tr["first"], " ", tr["second"]));
      _ = i.CreateIndexColumn(SqlDml.Concat(tr["second"], " ", tr["first"]));
      _ = ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[ExpressionIndexTableName];
      var i2 = t2.Indexes[ExpressionIndexName];
      Assert.That(i2, Is.Not.Null);
      Assert.That(i2.Columns.Count, Is.EqualTo(4));

      Assert.That(i2.Columns[2].Expression is not null, Is.True);
      Assert.That(i2.Columns[3].Expression is not null, Is.True);
    }

  }
}