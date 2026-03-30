// Copyright (C) 2010-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System.Data;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [TestFixture]
  public class ExceptionTypesTest : Sql.ExceptionTypesTest
  {
    private const string PgTimeoutTableName = "PgTheTimeout";
    private const string IdColumnName = "id";

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      Connection.BeginTransaction();
      EnsureTableNotExists(schema, PgTimeoutTableName);
      Connection.Commit();
    }

    [Test]
    public void PostgreSqlServerSideTimeout()
    {
      Connection.BeginTransaction();
      var table = schema.CreateTable(PgTimeoutTableName);
      _ = CreatePrimaryKey(table);
      _ = ExecuteNonQuery(SqlDdl.Create(table));
      Connection.Commit();

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.AddValueRow((tableRef[IdColumnName], 1));

      using (var connectionOne = Driver.CreateConnection()) {
        connectionOne.Open();
        connectionOne.BeginTransaction();
        using (var command = connectionOne.CreateCommand()) {
          command.CommandText = "SET statement_timeout = 15";
          _ = command.ExecuteNonQuery();
        }

        using (var connectionTwo = Driver.CreateConnection()) {
          connectionTwo.Open();
          connectionTwo.BeginTransaction(IsolationLevel.ReadCommitted);

          using (var command = connectionTwo.CreateCommand(insert)) {
            _ = command.ExecuteNonQuery();
          }
          AssertExceptionType(connectionOne, insert, SqlExceptionType.OperationTimeout);
        }
      }
    }
  }
}