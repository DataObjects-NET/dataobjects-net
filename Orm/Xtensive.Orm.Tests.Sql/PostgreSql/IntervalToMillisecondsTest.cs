// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public sealed class IntervalToMillisecondsTest : SqlTest
  {
    private const string IdColumnName = "Id";
    private const string ValueColumnName = "Value";
    private const string TableName = "IntervalToMsTest";

    private TypeMapping longMapping;
    private TypeMapping timeSpanMapping;
    private TypeMapping doubleMapping;

    private SqlSelect selectQuery;

    private static TimeSpan[] TestValues
    {
      get => new[] {
        TimeSpan.MinValue,
        TimeSpan.MaxValue,
        TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)),
        TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)),
        TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)),
        TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)),
        TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)),
        TimeSpan.FromMinutes(43).Add(TimeSpan.FromSeconds(43)),
        TimeSpan.FromMinutes(55).Add(TimeSpan.FromSeconds(55)),
        TimeSpan.FromMinutes(59).Add(TimeSpan.FromSeconds(59)),
        TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1))),
        TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10))),
        TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15))),
        TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27))),
        TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30))),
        TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)))),
        TimeSpan.FromDays(30).Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)))),
        TimeSpan.FromDays(15).Add(TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)))),
        TimeSpan.FromDays(20).Add(TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)))),
        TimeSpan.FromDays(23).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(28).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(29).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(32).Add(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)))),
        TimeSpan.FromDays(40).Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)))),
        TimeSpan.FromDays(65).Add(TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)))),
        TimeSpan.FromDays(181).Add(TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)))),
        TimeSpan.FromDays(182).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(360).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(363).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(364).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(365).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(366).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),
        TimeSpan.FromDays(730).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))),

        TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)).Negate(),
        TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)).Negate(),
        TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)).Negate(),
        TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)).Negate(),
        TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)).Negate(),
        TimeSpan.FromMinutes(43).Add(TimeSpan.FromSeconds(43)).Negate(),
        TimeSpan.FromMinutes(55).Add(TimeSpan.FromSeconds(55)).Negate(),
        TimeSpan.FromMinutes(59).Add(TimeSpan.FromSeconds(59)).Negate(),
        TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1))).Negate(),
        TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10))).Negate(),
        TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15))).Negate(),
        TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27))).Negate(),
        TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30))).Negate(),
        TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)))).Negate(),
        TimeSpan.FromDays(30).Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)))).Negate(),
        TimeSpan.FromDays(15).Add(TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)))).Negate(),
        TimeSpan.FromDays(20).Add(TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)))).Negate(),
        TimeSpan.FromDays(23).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(28).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(29).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(32).Add(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(1)))).Negate(),
        TimeSpan.FromDays(40).Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10)))).Negate(),
        TimeSpan.FromDays(65).Add(TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(15)))).Negate(),
        TimeSpan.FromDays(181).Add(TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(27).Add(TimeSpan.FromSeconds(27)))).Negate(),
        TimeSpan.FromDays(182).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(360).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(363).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(364).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(365).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(366).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
        TimeSpan.FromDays(730).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate()
      };
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      longMapping = Driver.TypeMappings[typeof(long)];
      timeSpanMapping = Driver.TypeMappings[typeof(TimeSpan)];
      doubleMapping = Driver.TypeMappings[typeof(double)];

      var dropTableCommand = Connection
        .CreateCommand(
          $"DROP TABLE IF EXISTS \"{TableName}\";");
      using (dropTableCommand) {
        _ = dropTableCommand.ExecuteNonQuery();
      }

      var createTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{TableName}\" (\"{IdColumnName}\" bigint CONSTRAINT PK_{TableName} PRIMARY KEY, \"{ValueColumnName}\" interval);");
      using (createTableCommand) {
        _ = createTableCommand.ExecuteNonQuery();
      }

      var schema = ExtractDefaultSchema();
      var tableRef = SqlDml.TableRef(schema.Tables[TableName]);
      var selectTotalMsQuery = SqlDml.Select(tableRef);
      selectTotalMsQuery.Columns.Add(tableRef[IdColumnName], "id");
      selectTotalMsQuery.Columns.Add(tableRef[ValueColumnName], "timespan");
      selectTotalMsQuery.Columns.Add(SqlDml.IntervalToMilliseconds(tableRef[ValueColumnName]), "totalMs");
      selectTotalMsQuery.Where = tableRef[IdColumnName] == SqlDml.ParameterRef("pId");
      selectQuery = selectTotalMsQuery;
    }

    protected override void TestFixtureTearDown()
    {
      longMapping = null;
      timeSpanMapping = null;
      doubleMapping = null;
      selectQuery = null;

      base.TestFixtureTearDown();
    }


    [Test]
    [TestCaseSource(nameof(TestValues))]
    public void MainTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }


    private void TestValue(TimeSpan testCase)
    {
      InsertValue(testCase.Ticks, testCase);
      var rowFromDb = SelectValue(testCase.Ticks);
      var trueTotalMilliseconds = testCase.TotalMilliseconds;
      var databaseValueTotalMilliseconds = rowFromDb.Item2.TotalMilliseconds;
      var extractedTotalMilliseconds = rowFromDb.Item3;

      Assert.That(databaseValueTotalMilliseconds, Is.EqualTo(trueTotalMilliseconds));
      Assert.That(extractedTotalMilliseconds, Is.EqualTo(trueTotalMilliseconds));
    }

    private void InsertValue(long id, TimeSpan testCase)
    {
      var command = Connection.CreateCommand($"INSERT INTO \"{TableName}\"(\"{IdColumnName}\", \"{ValueColumnName}\") VALUES (@pId, @pValue)");
      var pId = Connection.CreateParameter();
      pId.ParameterName = "pId";
      longMapping.BindValue(pId, id);
      _ = command.Parameters.Add(pId);

      var pValue = Connection.CreateParameter();
      pValue.ParameterName = "pValue";
      timeSpanMapping.BindValue(pValue, testCase);
      _ = command.Parameters.Add(pValue);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    private (long, TimeSpan, double) SelectValue(long id)
    {
      var command = Connection.CreateCommand(selectQuery);
      var pId = Connection.CreateParameter();
      pId.ParameterName = "pId";
      longMapping.BindValue(pId, id);
      _ = command.Parameters.Add(pId);

      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var idFromDb = (long) longMapping.ReadValue(reader, 0);
          var valueFromDb = (TimeSpan) timeSpanMapping.ReadValue(reader, 1);
          var totalMs = (double) doubleMapping.ReadValue(reader, 2);
          return (idFromDb, valueFromDb, totalMs);
        }
      }

      return default;
    }
  }
}
