// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests
{
  public abstract class ExceptionTypesTest : SqlTest
  {
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
    }

    [Test]
    public void SyntaxErrorTest()
    {
      AssertExceptionType("select lolwut??!", SqlExceptionType.SyntaxError);
    }

    [Test]
    public void CheckConstraintTest()
    {
      var table = schema.CreateTable(CheckedTableName);
      CreatePrimaryKey(table);
      var valueColumn1 = table.CreateColumn("value1", new SqlValueType(SqlType.Int32));
      valueColumn1.IsNullable = true;
      var valueColumn2 = table.CreateColumn("value2", new SqlValueType(SqlType.Int32));
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
    public void ReferentialConstraintTest()
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
      AssertExceptionType(slaveInsert, SqlExceptionType.ReferentialContraintViolation);

      var masterTableRef = SqlDml.TableRef(masterTable);
      var masterInsert = SqlDml.Insert(masterTableRef);
      masterInsert.Values.Add(masterTableRef[IdColumnName], 1);
      ExecuteNonQuery(masterInsert);
      ExecuteNonQuery(slaveInsert);

      var masterDelete = SqlDml.Delete(masterTableRef);
      masterDelete.Where = masterTableRef[IdColumnName]==1;
      AssertExceptionType(masterDelete, SqlExceptionType.ReferentialContraintViolation);
    }

    [Test]
    public void UniqueConstraintTestTest()
    {
      var table = schema.CreateTable(UniqueTableName);
      CreatePrimaryKey(table);
      var column = table.CreateColumn("value", new SqlValueType(SqlType.Int32));
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
    public void DeadlockTest()
    {
      var table = schema.CreateTable(DeadlockTableName);
      CreatePrimaryKey(table);
      var column = table.CreateColumn("value", new SqlValueType(SqlType.Int32));
      column.IsNullable = true;
      ExecuteNonQuery(SqlDdl.Create(table));

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);
      ExecuteNonQuery(insert);
      insert.Values.Clear();
      insert.Values.Add(tableRef[IdColumnName], 2);
      ExecuteNonQuery(insert);

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
      
      Connection.BeginTransaction(IsolationLevel.ReadCommitted);
      try {
        using (var anotherConnection = Driver.CreateConnection()) {
          anotherConnection.Open();
          anotherConnection.BeginTransaction(IsolationLevel.ReadCommitted);

          using (var command = Connection.CreateCommand(update1To1))
            command.ExecuteNonQuery();
          using (var command = anotherConnection.CreateCommand(update2To2))
            command.ExecuteNonQuery();

          var startEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
          var arg1 = new EvilThreadArgument {
            Connection = Connection,
            StartEvent = startEvent,
            Statement = update2To1
          };
          var arg2 = new EvilThreadArgument {
            Connection = anotherConnection,
            StartEvent = startEvent,
            Statement = update1To2
          };
          var thread1 = StartEvilThread(arg1);
          var thread2 = StartEvilThread(arg2);
          startEvent.Set();
          thread1.Join();
          thread2.Join();
          startEvent.Close();
          Assert.AreEqual(SqlExceptionType.Deadlock, arg1.ExceptionType ?? arg2.ExceptionType);
        }
      }
      finally {
        Connection.Rollback();
      }
    }

    [Test]
    public void TimeoutTest()
    {
      var table = schema.CreateTable(TimeoutTableName);
      CreatePrimaryKey(table);
      ExecuteNonQuery(SqlDdl.Create(table));

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[IdColumnName], 1);

      using (var anotherConnection = Driver.CreateConnection()) {
        anotherConnection.Open();
        anotherConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        using (var command = anotherConnection.CreateCommand(insert))
          command.ExecuteNonQuery();
        AssertExceptionType(insert, SqlExceptionType.OperationTimeout);
      }
    }

    [Test]
    public void SerializationFailureTest()
    {
      // How to implement this?
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
      };

      var thread = new Thread(entry);
      thread.Start();
      return thread;
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
      catch(Exception exception) {
        Assert.AreEqual(expectedExceptionType, Driver.GetExceptionType(exception));
        return;
      }
      Assert.Fail("Exception was not thrown");
    }

    private static TableColumn CreatePrimaryKey(Table table)
    {
      var column = table.CreateColumn(IdColumnName, new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("pk_" + table.Name, column);
      return column;
    }
  }
}