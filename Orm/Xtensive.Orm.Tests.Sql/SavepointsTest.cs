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
        Assert.AreEqual(1, GetCount());
        Connection.MakeSavepoint("sp1");
        DoInsert();
        Assert.AreEqual(2, GetCount());
        Connection.RollbackToSavepoint("sp1");
        Assert.AreEqual(1, GetCount());
        // we need to recreate savepoint after we rolled back to it
        Connection.MakeSavepoint("sp1"); 

        DoInsert();
        DoInsert();
        Assert.AreEqual(3, GetCount());
        Connection.MakeSavepoint("sp2");
        DoInsert();
        Assert.AreEqual(4, GetCount());
        Connection.MakeSavepoint("sp3");
        DoInsert();
        Assert.AreEqual(5, GetCount());
        Connection.RollbackToSavepoint("sp2");
        Assert.AreEqual(3, GetCount());
        Connection.RollbackToSavepoint("sp1");
        Assert.AreEqual(1, GetCount());

        Connection.Rollback();
        Assert.AreEqual(0, GetCount());
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
      insert.Values.Add(tableRef[IdColumn], nextId++);
      using (var command = Connection.CreateCommand(insert))
        command.ExecuteNonQuery();
    }
  }
}