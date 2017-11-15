// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class ExceptionTypesTest : SqlTest
  {
    private SqlConnection connectionOne;

    private class EvilThreadArgument
    {
      public SqlConnection Connection;
      public ISqlCompileUnit Statement;
      public WaitHandle StartEvent;
      public SqlExceptionType? ExceptionType;
    }

    private const string IdColumnName = "id";
    private const string TimeoutTableName = "TheTimeout";
    private const string DeadlockTableName = "TheDeadlock";
    private const string MasterTableName = "TheMaster";
    private const string SlaveTableName = "TheSlave";
    private const string UniqueTableName = "TheUnique";
    private const string CheckedTableName = "TheChecked";

    private Schema schema;

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      schema = ExtractDefaultSchema();
      EnsureTableNotExists(schema, TimeoutTableName);
      EnsureTableNotExists(schema, DeadlockTableName);
      EnsureTableNotExists(schema, SlaveTableName);
      EnsureTableNotExists(schema, MasterTableName);
      EnsureTableNotExists(schema, UniqueTableName);
      EnsureTableNotExists(schema, CheckedTableName);
      connectionOne = Connection;
    }

    public override void TearDown()
    {
      if (Connection.ActiveTransaction!=null)
        Connection.Rollback();
    }

    [Test]
    public virtual void SyntaxErrorTest()
    {
      AssertExceptionType("select lolwut??!", SqlExceptionType.SyntaxError);
    }

    [Test]
    public virtual void CheckConstraintTest()
    {
      var table = schema.CreateTable(CheckedTableName);
      CreatePrimaryKey(table);
      var valueColumn1 = table.CreateColumn("value1", Driver.TypeMappings[typeof (int)].MapType());
      valueColumn1.IsNullable = true;
      var valueColumn2 = table.CreateColumn("value2", Driver.TypeMappings[typeof (int)].MapType());
      valueColumn2.IsNullable = false;

      var tableRef = SqlDml.TableRef(table);
      table.CreateCheckConstraint("check_me", tableRef[valueColumn1.Name] > 0);

      ExecuteNonQuery(SqlDdl.Create(table));

      // violation of NOT NULL constraint
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);
      insert.Values.Add(tableRef[valueColumn1.Name], 1);
      insert.Values.Add(tableRef[valueColumn2.Name], SqlDml.Null);
      AssertExceptionType(insert, SqlExceptionType.CheckConstraintViolation);

      // violation of CHECK constraint
      insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 2);
      insert.Values.Add(tableRef[valueColumn1.Name], 0);
      insert.Values.Add(tableRef[valueColumn2.Name], 0);
      AssertExceptionType(insert, SqlExceptionType.CheckConstraintViolation);
    }

    [Test]
    public virtual void ReferentialConstraintTest()
    {
      var masterTable = schema.CreateTable(MasterTableName);
      var masterId = CreatePrimaryKey(masterTable);
      var slaveTable = schema.CreateTable(SlaveTableName);
      var slaveId = CreatePrimaryKey(slaveTable);
      var fk = slaveTable.CreateForeignKey("foreign_me");
      fk.ReferencedTable = masterTable;
      fk.ReferencedColumns.Add(masterId);
      fk.Columns.Add(slaveId);
      ExecuteNonQuery(SqlDdl.Create(masterTable));
      ExecuteNonQuery(SqlDdl.Create(slaveTable));

      var slaveTableRef = SqlDml.TableRef(slaveTable);
      var slaveInsert = SqlDml.Insert(slaveTableRef);
      slaveInsert.Values.Add(slaveTableRef[IdColumnName], 1);
      AssertExceptionType(slaveInsert, SqlExceptionType.ReferentialConstraintViolation);

      var masterTableRef = SqlDml.TableRef(masterTable);
      var masterInsert = SqlDml.Insert(masterTableRef);
      masterInsert.Values.Add(masterTableRef[IdColumnName], 1);
      ExecuteNonQuery(masterInsert);
      ExecuteNonQuery(slaveInsert);

      var masterDelete = SqlDml.Delete(masterTableRef);
      masterDelete.Where = masterTableRef[IdColumnName]==1;
      AssertExceptionType(masterDelete, SqlExceptionType.ReferentialConstraintViolation);
    }

    [Test]
    public virtual void UniqueConstraintTestTest()
    {
      var table = schema.CreateTable(UniqueTableName);
      CreatePrimaryKey(table);
      var column = table.CreateColumn("value", Driver.TypeMappings[typeof (int)].MapType());
      column.IsNullable = true;
      table.CreateUniqueConstraint("unique_me", column);
      ExecuteNonQuery(SqlDdl.Create(table));

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);

      // violation of PRIMARY KEY constraint
      ExecuteNonQuery(insert);
      AssertExceptionType(insert, SqlExceptionType.UniqueConstraintViolation);

      // violation of UNIQUE constraint
      insert.Values.Clear();
      insert.Values.Add(tableRef[IdColumnName], 2);
      insert.Values.Add(tableRef[column.Name], 0);
      ExecuteNonQuery(insert);

      insert.Values.Clear();
      insert.Values.Add(tableRef[IdColumnName], 3);
      insert.Values.Add(tableRef[column.Name], 0);
      AssertExceptionType(insert, SqlExceptionType.UniqueConstraintViolation);
    }

    [Test]
    public virtual void DeadlockTest()
    {
      var table = schema.CreateTable(DeadlockTableName);
      CreatePrimaryKey(table);
      var column = table.CreateColumn("value", Driver.TypeMappings[typeof (int)].MapType());
      column.IsNullable = true;
      ExecuteNonQuery(SqlDdl.Create(table));

      Connection.BeginTransaction();
      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);
      ExecuteNonQuery(insert);
      insert.Values.Clear();
      insert.Values.Add(tableRef[IdColumnName], 2);
      ExecuteNonQuery(insert);
      Connection.Commit();

      var update1To1 = SqlDml.Update(tableRef);
      update1To1.Where = tableRef[IdColumnName]==1;
      update1To1.Values.Add(tableRef[column.Name], 1);

      var update1To2 = SqlDml.Update(tableRef);
      update1To2.Where = tableRef[IdColumnName]==1;
      update1To2.Values.Add(tableRef[column.Name], 2);

      var update2To1 = SqlDml.Update(tableRef);
      update2To1.Where = tableRef[IdColumnName]==2;
      update2To1.Values.Add(tableRef[column.Name], 1);

      var update2To2 = SqlDml.Update(tableRef);
      update2To2.Where = tableRef[IdColumnName]==2;
      update2To2.Values.Add(tableRef[column.Name], 2);
      
      connectionOne.BeginTransaction(IsolationLevel.ReadCommitted);
      using (var connectionTwo = Driver.CreateConnection()) {
        connectionTwo.Open();
        connectionTwo.BeginTransaction(IsolationLevel.ReadCommitted);

        using (var command = connectionOne.CreateCommand(update1To1))
          command.ExecuteNonQuery();
        using (var command = connectionTwo.CreateCommand(update2To2))
          command.ExecuteNonQuery();

        var startEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        var arg1 = new EvilThreadArgument {
          Connection = connectionOne,
          StartEvent = startEvent,
          Statement = update2To1
        };
        var arg2 = new EvilThreadArgument {
          Connection = connectionTwo,
          StartEvent = startEvent,
          Statement = update1To2
        };
        var thread1 = StartEvilThread(arg1);
        var thread2 = StartEvilThread(arg2);
        startEvent.Set();
        thread1.Join();
        thread2.Join();
        startEvent.Close();
        var actual = arg1.ExceptionType ?? arg2.ExceptionType ?? SqlExceptionType.Unknown;
        AssertExceptionType(SqlExceptionType.Deadlock, actual);
      }
    }

    [Test]
    public virtual void TimeoutTest()
    {
      var table = schema.CreateTable(TimeoutTableName);
      CreatePrimaryKey(table);
      ExecuteNonQuery(SqlDdl.Create(table));

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);

      connectionOne.BeginTransaction();
      using (var connectionTwo = Driver.CreateConnection()) {
        connectionTwo.Open();
        connectionTwo.BeginTransaction(IsolationLevel.ReadCommitted);
        using (var command = connectionTwo.CreateCommand(insert))
          command.ExecuteNonQuery();
        AssertExceptionType(insert, SqlExceptionType.OperationTimeout);
      }
    }

    [Test, Ignore("Test is not implemented")]
    public virtual void SerializationFailureTest()
    {
    }

    private static Thread StartEvilThread(EvilThreadArgument arg)
    {
      ThreadStart entry = () => {
        arg.ExceptionType = null;
        try {
          using (var command = arg.Connection.CreateCommand(arg.Statement)) {
            arg.StartEvent.WaitOne();
            command.ExecuteNonQuery();
          }
        }
        catch (Exception exception) {
          arg.ExceptionType = arg.Connection.Driver.GetExceptionType(exception);
        }
        arg.Connection.Rollback();
      };

      var thread = new Thread(entry);
      thread.Start();
      return thread;
    }

    protected virtual void AssertExceptionType(SqlExceptionType expected, SqlExceptionType actual)
    {
      Assert.AreEqual(expected, actual);
    }

    private void AssertExceptionType(ISqlCompileUnit statement, SqlExceptionType expectedExceptionType)
    {
      var commandText = Driver.Compile(statement).GetCommandText();
      AssertExceptionType(commandText, expectedExceptionType);
    }

    private void AssertExceptionType(string commandText, SqlExceptionType expectedExceptionType)
    {
      try {
        ExecuteNonQuery(commandText);
      }
      catch (Exception exception) {
        AssertExceptionType(expectedExceptionType, Driver.GetExceptionType(exception));
        return;
      }
      Assert.Fail("Exception was not thrown");
    }

    private TableColumn CreatePrimaryKey(Table table)
    {
      var column = table.CreateColumn(IdColumnName, Driver.TypeMappings[typeof (int)].MapType());
      table.CreatePrimaryKey("pk_" + table.Name, column);
      return column;
    }
  }
}