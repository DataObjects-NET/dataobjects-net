// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.20

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class SavepointsTest : SqlTest
  {
    private const string IdColumn = "id";
    private const string TestTable = "savepoints_test";

    private int nextId;
    private SqlTableRef tableRef;
    private string countQuery;

    [Test]
    public void ComplexTest()
    {
      try {
        Connection.BeginTransaction();

        DoInsert();
        Assert.That(GetCount(), Is.EqualTo(1));
        Connection.MakeSavepoint("sp1");
        DoInsert();
        Assert.That(GetCount(), Is.EqualTo(2));
        Connection.RollbackToSavepoint("sp1");
        Assert.That(GetCount(), Is.EqualTo(1));
        // we need to recreate savepoint after we rolled back to it
        Connection.MakeSavepoint("sp1"); 

        DoInsert();
        DoInsert();
        Assert.That(GetCount(), Is.EqualTo(3));
        Connection.MakeSavepoint("sp2");
        DoInsert();
        Assert.That(GetCount(), Is.EqualTo(4));
        Connection.MakeSavepoint("sp3");
        DoInsert();
        Assert.That(GetCount(), Is.EqualTo(5));
        Connection.RollbackToSavepoint("sp2");
        Assert.That(GetCount(), Is.EqualTo(3));
        Connection.RollbackToSavepoint("sp1");
        Assert.That(GetCount(), Is.EqualTo(1));

        Connection.Rollback();
        Assert.That(GetCount(), Is.EqualTo(0));
      }
      finally {
        if (Connection.ActiveTransaction!=null)
          Connection.Rollback();
      }
    }

    protected override void  TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      var testSchema = ExtractDefaultSchema();
      Table table;
      try {
        Connection.BeginTransaction();
        EnsureTableNotExists(testSchema, TestTable);
        table = testSchema.CreateTable(TestTable);
        _ = table.CreateColumn(IdColumn, new SqlValueType(SqlType.Decimal, 10, 0));

        _ = ExecuteNonQuery(SqlDdl.Create(table));
        Connection.Commit();
      }
      catch {
        if (Connection.ActiveTransaction != null)
          Connection.Rollback();
        throw;
      }

      tableRef = SqlDml.TableRef(table);

      var select = SqlDml.Select(tableRef);
      select.Columns.Add(SqlDml.Count());
      countQuery = Connection.Driver.Compile(select).GetCommandText();
    }
    
    private int GetCount()
    {
      using (var command = Connection.CreateCommand(countQuery))
        return Convert.ToInt32(command.ExecuteScalar());
    }
    
    private void DoInsert()
    {
      var insert = SqlDml.Insert(tableRef);
      insert.AddValueRow((tableRef[IdColumn], nextId++));
      using (var command = Connection.CreateCommand(insert)) {
        _ = command.ExecuteNonQuery();
      }
    }
  }
}