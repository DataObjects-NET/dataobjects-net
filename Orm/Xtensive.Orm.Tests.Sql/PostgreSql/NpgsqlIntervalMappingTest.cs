// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  internal class NpgsqlIntervalMappingTest : SqlTest
  {
    private const string IdColumnName = "Id";
    private const string ValueColumnName = "Value";
    private const string TableName = "NpgsqlIntervalTest";

    #region Test case sources
    private static TimeSpan[] SecondsCases
    {
      get => new[] {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(27),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(43),
        TimeSpan.FromSeconds(55),
        TimeSpan.FromSeconds(59),

        TimeSpan.FromSeconds(1).Negate(),
        TimeSpan.FromSeconds(10).Negate(),
        TimeSpan.FromSeconds(15).Negate(),
        TimeSpan.FromSeconds(27).Negate(),
        TimeSpan.FromSeconds(30).Negate(),
        TimeSpan.FromSeconds(43).Negate(),
        TimeSpan.FromSeconds(55).Negate(),
        TimeSpan.FromSeconds(59).Negate(),
      };
    }

    private static TimeSpan[] MinutesCases
    {
      get => new[] {
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(27),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromMinutes(43),
        TimeSpan.FromMinutes(55),
        TimeSpan.FromMinutes(59),

        TimeSpan.FromMinutes(1).Negate(),
        TimeSpan.FromMinutes(10).Negate(),
        TimeSpan.FromMinutes(15).Negate(),
        TimeSpan.FromMinutes(27).Negate(),
        TimeSpan.FromMinutes(30).Negate(),
        TimeSpan.FromMinutes(43).Negate(),
        TimeSpan.FromMinutes(55).Negate(),
        TimeSpan.FromMinutes(59).Negate(),
      };
    }

    private static TimeSpan[] HoursCases
    {
      get => new[] {
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(10),
        TimeSpan.FromHours(15),
        TimeSpan.FromHours(20),
        TimeSpan.FromHours(23),

        TimeSpan.FromHours(1).Negate(),
        TimeSpan.FromHours(10).Negate(),
        TimeSpan.FromHours(15).Negate(),
        TimeSpan.FromHours(20).Negate(),
        TimeSpan.FromHours(23).Negate(),
      };
    }

    private static TimeSpan[] LessThanMonthCases
    {
      get => new[] {
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(10),
        TimeSpan.FromDays(15),
        TimeSpan.FromDays(20),
        TimeSpan.FromDays(23),
        TimeSpan.FromDays(28),
        TimeSpan.FromDays(29),

        TimeSpan.FromDays(1).Negate(),
        TimeSpan.FromDays(10).Negate(),
        TimeSpan.FromDays(15).Negate(),
        TimeSpan.FromDays(20).Negate(),
        TimeSpan.FromDays(23).Negate(),
        TimeSpan.FromDays(28).Negate(),
        TimeSpan.FromDays(29).Negate(),
      };
    }

    private static TimeSpan[] MoreThanMonthCases
    {
      get => new[] {
        TimeSpan.FromDays(32),
        TimeSpan.FromDays(40),
        TimeSpan.FromDays(65),
        TimeSpan.FromDays(181),
        TimeSpan.FromDays(182),
        TimeSpan.FromDays(360),
        TimeSpan.FromDays(363),
        TimeSpan.FromDays(364),
        TimeSpan.FromDays(365),
        TimeSpan.FromDays(366),
        TimeSpan.FromDays(730),

        TimeSpan.FromDays(32).Negate(),
        TimeSpan.FromDays(40).Negate(),
        TimeSpan.FromDays(65).Negate(),
        TimeSpan.FromDays(181).Negate(),
        TimeSpan.FromDays(182).Negate(),
        TimeSpan.FromDays(360).Negate(),
        TimeSpan.FromDays(363).Negate(),
        TimeSpan.FromDays(364).Negate(),
        TimeSpan.FromDays(365).Negate(),
        TimeSpan.FromDays(366).Negate(),
        TimeSpan.FromDays(730).Negate(),
      };
    }

    private static TimeSpan[] MultipartValuesSource
    {
      get => new[] {
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
        TimeSpan.FromDays(730).Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(30)))).Negate(),
      };
    }
    #endregion

    private TypeMapping longMapping;
    private TypeMapping timeSpanMapping;


    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      longMapping = Driver.TypeMappings[typeof(long)];
      timeSpanMapping = Driver.TypeMappings[typeof(TimeSpan)];

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
    }

    protected override void TestFixtureTearDown()
    {
      longMapping = null;
      timeSpanMapping = null;

      base.TestFixtureTearDown();
    }

    [Test]
    [TestCaseSource(nameof(MultipartValuesSource))]
    public void MultipartValueTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }

    [Test]
    [TestCaseSource(nameof(SecondsCases))]
    public void SecondsTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }


    [Test]
    [TestCaseSource(nameof(MinutesCases))]
    public void MinutesTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }

    [Test]
    [TestCaseSource(nameof(HoursCases))]
    public void HoursTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }

    [Test]
    [TestCaseSource(nameof(LessThanMonthCases))]
    public void DaysTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }

    [Test]
    [TestCaseSource(nameof(MoreThanMonthCases))]
    public void DaysMoreThanMonthTest(TimeSpan testCase)
    {
      TestValue(testCase);
    }


    private void TestValue(TimeSpan testCase)
    {
      InsertValue(testCase.Ticks, testCase);
      var rowFromDb = SelectValue(testCase.Ticks);

      Assert.That(TimeSpan.FromTicks(rowFromDb.Item1), Is.EqualTo(testCase));
      Assert.That(rowFromDb.Item2, Is.EqualTo(testCase));
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

    private (long, TimeSpan) SelectValue(long id)
    {
      var command = Connection.CreateCommand($"SELECT \"{IdColumnName}\", \"{ValueColumnName}\" FROM \"{TableName}\" WHERE \"{IdColumnName}\" = @pId");
      var pId = Connection.CreateParameter();
      pId.ParameterName = "pId";
      longMapping.BindValue(pId, id);
      _ = command.Parameters.Add(pId);

      TimeSpan result = default;
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read() || result == default) {
          var idFromDb = (long) longMapping.ReadValue(reader, 0);
          var valueFromDb = (TimeSpan) timeSpanMapping.ReadValue(reader, 1);
          return (idFromDb, valueFromDb);
        }
      }

      return default;
    }
  }
}
