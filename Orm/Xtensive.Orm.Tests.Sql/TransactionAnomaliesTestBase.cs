// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public abstract class TransactionAnomaliesTestBase : SqlTest
  {
    private const string TestTableName = "IsolationLevelTestTable";

    protected Schema defaultSchema;
    protected Table testTable;

    protected override void CheckRequirements()
      => Require.ProviderIsNot(StorageProvider.Sqlite, "No support for parallel connections to single database");

    #region Prepare schema

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      defaultSchema = ExtractDefaultSchema();
      testTable = CreateDatabaseStructure();

      Connection.Close();
    }

    protected override void TestFixtureTearDown()
    {
      try {
        Connection.Open();
        if (defaultSchema.Tables.Any(t => t.DbName == TestTableName)) {
          var existingTable = defaultSchema.Tables.First(t => t.DbName == TestTableName);
          DropTestTable(existingTable);
        }
      }
      catch { }
      finally {
        Connection.Close();
        base.TestFixtureTearDown();
      }
    }

    public override void SetUp()
    {
      Connection.Open();

      try {
        InsertRows();
      }
      finally {
        Connection.Close();
      }
    }

    public override void TearDown()
    {
      Connection.Open();
      try {
        DeleteRows();
      }
      finally {
        Connection.Close();
      }
    }

    private Table CreateDatabaseStructure()
    {
      if (defaultSchema.Tables.Any(t => t.DbName.Equals(TestTableName, StringComparison.OrdinalIgnoreCase))) {
        var existingTable = defaultSchema.Tables.First(t => t.DbName.Equals(TestTableName, StringComparison.OrdinalIgnoreCase));
        DropTestTable(existingTable);
        _ = defaultSchema.Tables.Remove(existingTable);
      }

      var testTable = defaultSchema.CreateTable(TestTableName);
      var idColumn = testTable.CreateColumn("Id", Driver.TypeMappings[typeof(int)].MapType());
      idColumn.IsNullable = false;
      var balanceColumn = testTable.CreateColumn("Balance", Driver.TypeMappings[typeof(int)].MapType());
      _ = testTable.CreatePrimaryKey("PK_IsolationLevelTestTable", idColumn);

      using (var createTableCommand = this.Connection.CreateCommand(SqlDdl.Create(testTable))) {
        _ = createTableCommand.ExecuteNonQuery();
      }
      return testTable;
    }

    private void InsertRows()
    {
      var inserts = new List<ISqlCompileUnit>();
      var tableRef = SqlDml.TableRef(testTable);
      for (var i = 0; i < 5; i++) {
        var insertStatement = SqlDml.Insert(SqlDml.TableRef(testTable));
        insertStatement.AddValueRow((tableRef[0], SqlDml.Literal(i)), (tableRef[1], SqlDml.Literal(i * 100)));

        inserts.Add(insertStatement);
      }

      Connection.BeginTransaction();
      try {
        foreach (var insert in inserts) {
          _ = ExecuteNonQuery(insert);
        }
        Connection.Commit();
      }
      catch (Exception) {
        Connection.Rollback();
        throw;
      }
    }
    private void DeleteRows()
    {
      Connection.BeginTransaction();
      try {
        var deleteAll = SqlDml.Delete(SqlDml.TableRef(testTable));
        _ = ExecuteNonQuery(deleteAll);
        Connection.Commit();
      }
      catch (Exception) {
        Connection.Rollback();
      }
    }

    private void DropTestTable(Table existingTable)
    {
      var dropTable = SqlDdl.Drop(existingTable);
      _ = ExecuteNonQuery(dropTable);
    }

    #endregion

    #region DataObjects.Net driver based implementation of anomalies (isolation level adjustments applied if needed)

    protected async Task<bool> TryPerformDirtyRead(IsolationLevel isolationLevel)
    {
      var t1Step1Done = new TaskCompletionSource<bool>();
      var t2Step1Done = new TaskCompletionSource<bool>();
      var t2Step2Done = new TaskCompletionSource<bool>();

      int? t2ObservedBalance = null;
      Exception t2Exception = null;
      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - writes but not commits
      var task1 = Task.Run(async () => {
        var update = SqlDml.Update(SqlDml.TableRef(testTable));
        update.Values[update.Update[1]] = SqlDml.Literal(rowId * 1000);
        update.Where = update.Update[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync();
          try {
            using (var cmd = conn.CreateCommand(update)) {
              cmd.CommandTimeout = 3;

              _ = await t2Step1Done.Task; //waits until T2 first read
              // Change row in database
              _ = await cmd.ExecuteNonQueryAsync();
            }
            // Signal t2 do it's work
            t1Step1Done.SetResult(true);

            // wait for t2
            _ = await t2Step2Done.Task;
          }
          finally {
            // rollback changes
            await conn.RollbackAsync();
            await conn.CloseAsync();
          }
        }
      });

      // T2 - tries to read
      var task2 = Task.Run(async () => {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(select.From[1]);
        select.Where = select.From[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync(isolationLevel);// DO level
          
          try {
            var queryText = Driver.Compile(select).GetCommandText();
            using (var cmd = conn.CreateCommand(queryText)) {
              cmd.CommandTimeout = 3;
              t2ObservedBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            t2Step1Done.SetResult(true);// allows T1 perform update

            // Wait until t1 does it's Update
            _ = await t1Step1Done.Task;

            // Read the balance once again
            try {
              using (var cmd = conn.CreateCommand(queryText)) {
                cmd.CommandTimeout = 3;
                t2ObservedBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync());
              }
            }
            catch (Exception ex) {
              t2Exception = ex; // in case of lock timeout exception
            }
            finally {
              // finished, allow t1 continue
              await conn.CommitAsync();
              t2Step2Done.SetResult(true);
            }

          }
          finally {
            await conn.CloseAsync();
          }
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2ObservedBalance == rowId * 1000);
    }

    protected async Task<bool> TryPerformFuzzyRead(IsolationLevel isolationLevel)
    {
      var t1FirstReadDone = new TaskCompletionSource<bool>();
      var t2UpdateDone = new TaskCompletionSource<bool>();

      int? t1InitialBalance = null;
      int? t1SecondBalance = null;
      Exception t2Exception = null;

      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - reads twice, checks data integrity
      var task1 = Task.Run(async () =>
      {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(select.From[1]);
        select.Where = select.From[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync(isolationLevel);
          try {
            var queryText = Driver.Compile(select).GetCommandText();

            using (var cmd = conn.CreateCommand(queryText)) {
              cmd.CommandTimeout = 3;
              t1InitialBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            t1FirstReadDone.SetResult(true);

            _ = await t2UpdateDone.Task;
            try {
              using (var cmd = conn.CreateCommand(queryText)) {
                cmd.CommandTimeout = 3;
                t1SecondBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync());
              }
            }
            catch (Exception) {
              await conn.RollbackAsync();
            }
            finally {
              await conn.CommitAsync();
            }
          }
          finally {
            await conn.CloseAsync();
          }
        }
      });

      // T2 - interfiers with changes
      var task2 = Task.Run(async () =>
      {
        var update = SqlDml.Update(SqlDml.TableRef(testTable));
        update.Values[update.Update[1]] = SqlDml.Literal(rowId * 1000);
        update.Where = update.Update[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync();

          _ = await t1FirstReadDone.Task;

          try {
            using (var cmd = conn.CreateCommand(update)) {
              cmd.CommandTimeout = 3;
              _ = await cmd.ExecuteNonQueryAsync();
            }
            await conn.CommitAsync();
          }
          catch (Exception ex) {
            t2Exception = ex;
            await conn.RollbackAsync();
          }
          finally {
            t2UpdateDone.SetResult(true);
          }

          await conn.CloseAsync();
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2Exception is null && t1SecondBalance == rowId * 1000);
    }

    protected async Task<bool> TryPerfromPhantomRead(IsolationLevel isolationLevel)
    {
      var t1FirstSelectDone = new TaskCompletionSource<bool>();
      var t2InsertDone = new TaskCompletionSource<bool>();

      int t1FirstCount = 0;
      int t1SecondCount = 0;
      Exception t2Exception = null;

      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - reads number of rows twice
      var task1 = Task.Run(async () =>
      {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(SqlDml.Count(SqlDml.Asterisk), "count");
        select.Where = select.From[1] > SqlDml.Literal(50);

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync(isolationLevel);

          try {
            var queryText = Driver.Compile(select).GetCommandText();

            using (var cmd = conn.CreateCommand(queryText)) {
              cmd.CommandTimeout = 3;
              t1FirstCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            t1FirstSelectDone.SetResult(true);
            _ = await t2InsertDone.Task;

            // Second reading
            try {
              using (var cmd = conn.CreateCommand(queryText)) {
                cmd.CommandTimeout = 3;
                t1SecondCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
              }
            }
            catch (Exception) {
              await conn.RollbackAsync();
            }
            finally {
              await conn.CommitAsync();
            }
          }
          finally {
            await conn.CloseAsync();
          }
        }
      });

      // T2 - Inserts new row  which fits the filter condition in T1
      var task2 = Task.Run(async () =>
      {
        var insert = SqlDml.Insert(SqlDml.TableRef(testTable));
        insert.AddValueRow((insert.Into[0], SqlDml.Literal(6)), (insert.Into[1], SqlDml.Literal(6 * 100)));

        using (var conn = Driver.CreateConnection()) {
          await conn.OpenAsync(default);
          await conn.BeginTransactionAsync();

          _ = await t1FirstSelectDone.Task;

          try {
            // Adds new row that fits into filter condition of T1 (balance > 50)
            using (var cmd = conn.CreateCommand(insert)) {
              cmd.CommandTimeout = 3;
              _ = await cmd.ExecuteNonQueryAsync();
            }
            await conn.CommitAsync();
          }
          catch (Exception ex) {
            t2Exception = ex;
            await conn.RollbackAsync();
          }
          finally {
            t2InsertDone.SetResult(true);
          }
          await conn.CloseAsync();
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2Exception is null && t1SecondCount > t1FirstCount);
    }

    #endregion

    #region ADO.NET based implementation of anomalies

    protected async Task<bool> TryPerformDirtyReadNative(IsolationLevel isolationLevel)
    {
      var t1Step1Done = new TaskCompletionSource<bool>();
      var t2Step1Done = new TaskCompletionSource<bool>();
      var t2Step2Done = new TaskCompletionSource<bool>();

      int? t2ObservedBalance = null;
      Exception t2Exception = null;
      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - writes but not commits
      var task1 = Task.Run(async () => {
        var update = SqlDml.Update(SqlDml.TableRef(testTable));
        update.Values[update.Update[1]] = SqlDml.Literal(rowId * 1000);
        update.Where = update.Update[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          // this is intentional, not recommended though
          var nativeConnection = conn.UnderlyingConnection;
          await nativeConnection.OpenAsync();

          using (var tx = await nativeConnection.BeginTransactionAsync()) {
            try {
              var queryText = Driver.Compile(update).GetCommandText();

              _ = await t2Step1Done.Task;
              using (var cmd = nativeConnection.CreateCommand()) {
                cmd.CommandText = queryText;
                cmd.Transaction = tx;
                cmd.CommandTimeout = 3;

                _ = await cmd.ExecuteNonQueryAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);

                t1Step1Done.SetResult(true);

                _ = await t2Step2Done.Task;
              }
            }
            finally {
              await tx.RollbackAsync();
              await nativeConnection.CloseAsync();
            }
          }
        }
      });

      // T2 - tries to read
      var task2 = Task.Run(async () => {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(select.From[1]);
        select.Where = select.From[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {

          var nativeConnection = conn.UnderlyingConnection;
          await nativeConnection.OpenAsync();

          using (var tx = await nativeConnection.BeginTransactionAsync(isolationLevel)) {
            var queryText = Driver.Compile(select).GetCommandText();
            try {
              using (var cmd = nativeConnection.CreateCommand()) {
                cmd.CommandText = queryText;
                cmd.Transaction = tx;
                cmd.CommandTimeout = 3;
                t2ObservedBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token));
              }
              t2Step1Done.SetResult(true);

              _ = await t1Step1Done.Task;

              try {
                using (var cmd = nativeConnection.CreateCommand()) {
                  cmd.CommandText = queryText;
                  cmd.Transaction = tx;
                  cmd.CommandTimeout = 3;
                  t2ObservedBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token));
                }
              }
              catch (Exception ex) {
                t2Exception = ex;
                await tx.RollbackAsync();
              }
              finally {
                await tx.CommitAsync();
                t2Step2Done.SetResult(true);
              }

            }
            finally {
              await nativeConnection.CloseAsync();
            }
          }
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2ObservedBalance == rowId * 1000);
    }

    protected async Task<bool> TryPerformFuzzyReadNative(IsolationLevel isolationLevel)
    {
      var t1FirstReadDone = new TaskCompletionSource<bool>();
      var t2UpdateDone = new TaskCompletionSource<bool>();

      int? t1InitialBalance = null;
      int? t1SecondBalance = null;
      Exception t2Exception = null;

      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - reads twice, checks data integrity
      var task1 = Task.Run(async () => {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(select.From[1]);
        select.Where = select.From[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          var nativeConnection = conn.UnderlyingConnection;

          await nativeConnection.OpenAsync();

          using (var tx = await nativeConnection.BeginTransactionAsync(isolationLevel)) {
            var queryText = Driver.Compile(select).GetCommandText();

            try {
              using (var cmd = nativeConnection.CreateCommand()) {
                cmd.CommandText = queryText;
                cmd.Transaction = tx;
                cmd.CommandTimeout = 3;

                t1InitialBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token));
              }

              t1FirstReadDone.SetResult(true);

              _ = await t2UpdateDone.Task;
              try {
                using (var cmd = nativeConnection.CreateCommand()) {
                  cmd.CommandText = queryText;
                  cmd.Transaction = tx;
                  cmd.CommandTimeout = 3;
                  t1SecondBalance = Convert.ToInt32(await cmd.ExecuteScalarAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token));
                }
              }
              catch (Exception) {
                await tx.RollbackAsync();
              }
              finally {
                await tx.CommitAsync();
              }
            }
            finally {
              await nativeConnection.CloseAsync();
            }
          }
        }
      });

      // T2 - interfiers with changes
      var task2 = Task.Run(async () => {
        var update = SqlDml.Update(SqlDml.TableRef(testTable));
        update.Values[update.Update[1]] = SqlDml.Literal(rowId * 1000);
        update.Where = update.Update[0] == SqlDml.Literal(rowId);

        using (var conn = Driver.CreateConnection()) {
          var nativeConnection = conn.UnderlyingConnection;
          await nativeConnection.OpenAsync();

          try {
            using (var tx = await nativeConnection.BeginTransactionAsync()) {
              var queryText = Driver.Compile(update).GetCommandText();
              _ = await t1FirstReadDone.Task;

              try {
                using (var cmd = nativeConnection.CreateCommand()) {
                  cmd.CommandText = queryText;
                  cmd.Transaction = tx;
                  cmd.CommandTimeout = 3;
                  _ = await cmd.ExecuteNonQueryAsync();
                }
                await tx.CommitAsync();
              }
              catch (Exception ex) {
                t2Exception = ex;
                await tx.RollbackAsync();
              }
              finally {
                t2UpdateDone.SetResult(true);
              }
            }
          }
          finally {
            await nativeConnection.CloseAsync();
          }
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2Exception is null && t1SecondBalance == rowId * 1000);
    }

    protected async Task<bool> TryPerfromPhantomReadNative(IsolationLevel isolationLevel)
    {
      var t1FirstSelectDone = new TaskCompletionSource<bool>();
      var t2InsertDone = new TaskCompletionSource<bool>();

      int t1FirstCount = 0;
      int t1SecondCount = 0;
      Exception t2Exception = null;

      var rowId = MapIsolationLevelToTableRow(isolationLevel);

      // T1 - reads number of rows twice
      var task1 = Task.Run(async () => {
        var select = SqlDml.Select(SqlDml.TableRef(testTable));
        select.Columns.Add(SqlDml.Count(SqlDml.Asterisk), "count");
        select.Where = select.From[1] > SqlDml.Literal(50);

        using (var conn = Driver.CreateConnection()) {
          var nativeConnection = conn.UnderlyingConnection;
          await nativeConnection.OpenAsync();

          try {
            using (var tx = await nativeConnection.BeginTransactionAsync(isolationLevel)) {
              var queryText = Driver.Compile(select).GetCommandText();
              using (var cmd = nativeConnection.CreateCommand()) {
                cmd.CommandText = queryText;
                cmd.Transaction = tx;
                cmd.CommandTimeout = 3;

                t1FirstCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
              }

              t1FirstSelectDone.SetResult(true);
              _ = await t2InsertDone.Task;

              try {
                using (var cmd = nativeConnection.CreateCommand()) {
                  cmd.CommandText = queryText;
                  cmd.Transaction = tx;
                  cmd.CommandTimeout = 3;
                  t1SecondCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
              }
              catch (Exception) {
                tx.Rollback();
              }
              finally {
                await tx.CommitAsync();
              }
            }
          }
          finally {
            await nativeConnection.CloseAsync();
          }
        }
      });

      // T2 - Inserts new row  which fits the filter condition in T1
      var task2 = Task.Run(async () => {
        var insert = SqlDml.Insert(SqlDml.TableRef(testTable));
        insert.AddValueRow((insert.Into[0], SqlDml.Literal(6)), (insert.Into[1], SqlDml.Literal(6 * 100)));

        using (var conn = Driver.CreateConnection()) {
          var nativeConnection = conn.UnderlyingConnection;
          await nativeConnection.OpenAsync();
          using (var tx = await nativeConnection.BeginTransactionAsync()) {
            var queryText = Driver.Compile(insert).GetCommandText();

            _ = await t1FirstSelectDone.Task;

            try {
              using (var cmd = nativeConnection.CreateCommand()) {
                cmd.CommandText = queryText;
                cmd.Transaction = tx;
                cmd.CommandTimeout = 3;
                _ = await cmd.ExecuteNonQueryAsync();
              }
              await tx.CommitAsync();
            }
            catch (Exception ex) {
              t2Exception = ex;
              await tx.RollbackAsync();
            }
            finally {
              t2InsertDone.SetResult(true);
            }
          }
        }
      });

      await Task.WhenAll(task1, task2);

      return (t2Exception is null && t1SecondCount > t1FirstCount);
    }

    #endregion

    private static int MapIsolationLevelToTableRow(IsolationLevel isolationLevel)
    {
      return isolationLevel switch {
        IsolationLevel.ReadUncommitted => 1,
        IsolationLevel.ReadCommitted => 2,
        IsolationLevel.RepeatableRead => 3,
        IsolationLevel.Serializable => 4,
        IsolationLevel.Snapshot => 5,
        _ => throw new NotSupportedException()
      };
    }
  }
}
