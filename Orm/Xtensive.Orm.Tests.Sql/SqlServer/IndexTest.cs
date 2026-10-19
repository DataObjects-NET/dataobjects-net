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

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class IndexTest2005 : IndexTest
  {
    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    protected override void CreateExpressionIndexTable(Schema schema)
    {
      Table t;
      t = schema.CreateTable(ExpressionIndexTableName);
      _ = t.CreateColumn("first", new SqlValueType(SqlType.VarChar, 50));
      _ = t.CreateColumn("second", new SqlValueType(SqlType.VarChar, 50));
      var c1 = t.CreateColumn("third", new SqlValueType(SqlType.VarChar));
      var c2 = t.CreateColumn("forth", new SqlValueType(SqlType.VarChar));

      var tr = SqlDml.TableRef(t);
      c1.Expression = SqlDml.Concat(tr["first"], " ", tr["second"]);
      c1.IsPersisted = false;
      c1.IsNullable = true;

      c2.Expression = SqlDml.Concat(tr["second"], " ", tr["first"]);
      c2.IsPersisted = false;
      c2.IsNullable = true;

      _ = ExecuteNonQuery(SqlDdl.Create(t));
    }

    [Test]
    public override void CreateExpressionIndexTest()
    {
      base.CreateExpressionIndexTest();

      // Creating index
      var t = ExpressionIndexTable;
      var i = t.CreateIndex(ExpressionIndexName);
      _ = i.CreateIndexColumn(t.TableColumns["third"]);
      _ = i.CreateIndexColumn(t.TableColumns["forth"]);
      _ = ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[ExpressionIndexTableName];
      var i2 = t2.Indexes[ExpressionIndexName];
      Assert.That(i2, Is.Not.Null);
      Assert.That(i2.Columns.Count, Is.EqualTo(2));

      Assert.That(t2.TableColumns["third"].Expression is not null, Is.True);
      Assert.That(t2.TableColumns["forth"].Expression is not null, Is.True);
    }
  }
}