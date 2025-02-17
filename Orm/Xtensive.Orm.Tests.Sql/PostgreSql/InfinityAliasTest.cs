// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public sealed class InfinityAliasTest : SqlTest
  {
    private const string DateOnlyMinValueTable = "DateOnlyTable1";
    private const string DateOnlyMaxValueTable = "DateOnlyTable2";

    private const string DateTimeMinValueTable = "DateTimeTable1";
    private const string DateTimeMaxValueTable = "DateTimeTable2";

    private const string DateTimeOffsetMinValueTable = "DateTimeOffsetTable1";
    private const string DateTimeOffsetMaxValueTable = "DateTimeOffsetTable2";

    private Xtensive.Sql.TypeMapping longTypeMapping;
    private Xtensive.Sql.TypeMapping dateOnlyTypeMapping;
    private Xtensive.Sql.TypeMapping dateTimeTypeMapping;
    private Xtensive.Sql.TypeMapping dateTimeOffsetTypeMapping;

    protected override void CheckRequirements()
    {
      // DO NOT check provider here.
      // Require class uses driver creation which casues AppContext switch setup before TestFixtureSetup() method called
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      Require.ProviderIs(StorageProvider.PostgreSql);

      longTypeMapping = Driver.TypeMappings[typeof(long)];
      dateOnlyTypeMapping = Driver.TypeMappings[typeof(DateOnly)];
      dateTimeTypeMapping = Driver.TypeMappings[typeof(DateTime)];
      dateTimeOffsetTypeMapping = Driver.TypeMappings[typeof(DateTimeOffset)];

      DropTablesForDateTime();
      DropTablesForDateOnly();
      DropTablesForDateTimeOffset();

      CreateTablesForDateTimeTests();
      CreateTablesForDateOnlyTests();
      CreateTablesForDateTimeOffsetTests();
    }

    [Test]
    public void MinDateTimeSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = reader.GetDateTime(1);
          Assert.That(datetimeValue, Is.EqualTo(DateTime.MinValue));
        }
      }
    }

    [Test]
    public void MinDateTimeSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeMinValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(filterP, DateTime.MinValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateTimeSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateTimeSelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateTimeSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateTimeSelectDatePart();
    }

    private void TestMinDateTimeSelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Day));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Hour));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Minute));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.Not.EqualTo(DateTime.MinValue.Second));
        }
      }
    }


    [Test]
    public void MaxDateTimeSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = reader.GetDateTime(1);
          var difference = (datetimeValue - DateTime.MaxValue).Duration();
          Assert.That(difference, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(0.001)));
        }
      }
    }

    [Test]
    public void MaxDateTimeSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeMaxValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(filterP, DateTime.MaxValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateTimeSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateTimeSelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateTimeSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateTimeSelectDatePart();
    }

    private void TestMaxDateTimeSelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Day));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Hour));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Minute));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateTime.MaxValue.Second));
        }
      }
    }


    [Test]
    public void MinDateOnlyNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateOnlyMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = DateOnly.FromDateTime(reader.GetDateTime(1));
          Assert.That(datetimeValue, Is.EqualTo(DateOnly.MinValue));
        }
      }
    }

    [Test]
    public void MinDateOnlyByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateOnlyMinValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateOnlyTypeMapping.BindValue(filterP, DateOnly.MinValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateOnlySelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateOnlySelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateOnlySelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateOnlySelectDatePart();
    }

    private void TestMinDateOnlySelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MinValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MinValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MinValue.Day));
        }
      }
    }


    [Test]
    public void MaxDateOnlyNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateOnlyMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = DateOnly.FromDateTime(reader.GetDateTime(1));
          Assert.That(datetimeValue, Is.EqualTo(DateOnly.MaxValue));
        }
      }
    }

    [Test]
    public void MaxDateOnlyByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateOnlyMaxValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateOnlyTypeMapping.BindValue(filterP, DateOnly.MaxValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateOnlySelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateOnlySelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateOnlySelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateOnlySelectDatePart();
    }

    private void TestMaxDateOnlySelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MaxValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MaxValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetDouble(0);
          Assert.That(yearPart, Is.EqualTo(DateOnly.MaxValue.Day));
        }
      }
    }


    [Test]
    public void MinDateTimeOffsetSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = reader.GetDateTime(1);
          Assert.That(datetimeValue, Is.EqualTo(DateTimeOffset.MinValue.DateTime));
        }
      }
    }

    [Test]
    public void MinDateTimeOffsetSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeOffsetMinValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(filterP, DateTimeOffset.MinValue);
      command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateTimeOffsetSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateTimeOffsetSelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MinDateTimeOffsetSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateTimeOffsetSelectDatePart();
    }

    private void TestMinDateTimeOffsetSelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Day));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Hour));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Minute));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MinValue.Second));
        }
      }
    }


    [Test]
    public void MaxDateTimeOffsetSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var datetimeValue = reader.GetDateTime(1);
          var difference = (datetimeValue - DateTimeOffset.MaxValue).Duration();
          Assert.That(difference, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(0.001)));
        }
      }
    }

    [Test]
    public void MaxDateTimeOffsetSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeOffsetMaxValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(filterP, DateTimeOffset.MaxValue);
      command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateTimeOffsetSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateTimeOffsetSelectDatePart();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void MaxDateTimeOffsetSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateTimeOffsetSelectDatePart();
    }

    private void TestMaxDateTimeOffsetSelectDatePart()
    {
      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Year));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Month));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Day));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Hour));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Minute));
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");

      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var yearPart = reader.GetInt64(0);
          Assert.That(yearPart, Is.EqualTo(DateTimeOffset.MaxValue.Second));
        }
      }
    }

    #region Create structure and populate data
    private void CreateTablesForDateTimeTests()
    {
      var createDateTimeTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeMinValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeMinValueTable} PRIMARY KEY, \"Value\" timestamp);");
      using (createDateTimeTableCommand) {
        _ = createDateTimeTableCommand.ExecuteNonQuery();
      }

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeMinValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, 1L);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, DateTime.MinValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }

      createDateTimeTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeMaxValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeMaxValueTable} PRIMARY KEY, \"Value\" timestamp);");
      using (createDateTimeTableCommand) {
        _ = createDateTimeTableCommand.ExecuteNonQuery();
      }

      command = Connection.CreateCommand($"INSERT INTO \"{DateTimeMaxValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, 2L);
      _ = command.Parameters.Add(p1);

      p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, DateTime.MaxValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    private void CreateTablesForDateOnlyTests()
    {
      var createDateOnlyTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateOnlyMinValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateOnlyMinValueTable} PRIMARY KEY, \"Value\" date);");
      using (createDateOnlyTableCommand) {
        _ = createDateOnlyTableCommand.ExecuteNonQuery();
      }

      var command = Connection.CreateCommand($"INSERT INTO \"{DateOnlyMinValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, 1L);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateOnlyTypeMapping.BindValue(p2, DateOnly.MinValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }

      createDateOnlyTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateOnlyMaxValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateOnlyMaxValueTable} PRIMARY KEY, \"Value\" date);");
      using (createDateOnlyTableCommand) {
        _ = createDateOnlyTableCommand.ExecuteNonQuery();
      }

      command = Connection.CreateCommand($"INSERT INTO \"{DateOnlyMaxValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, 2L);
      _ = command.Parameters.Add(p1);

      p2 = Connection.CreateParameter();
      dateOnlyTypeMapping.BindValue(p2, DateOnly.MaxValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    private void CreateTablesForDateTimeOffsetTests()
    {
      var createDateTimeOffsetTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeOffsetMinValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeOffsetMinValueTable} PRIMARY KEY, \"Value\" timestamptz);");
      using (createDateTimeOffsetTableCommand) {
        _ = createDateTimeOffsetTableCommand.ExecuteNonQuery();
      }

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetMinValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, 1L);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      p2.Value = DateTimeOffset.MinValue;
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }

      createDateTimeOffsetTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeOffsetMaxValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeOffsetMaxValueTable} PRIMARY KEY, \"Value\" timestamptz);");
      using (createDateTimeOffsetTableCommand) {
        _ = createDateTimeOffsetTableCommand.ExecuteNonQuery();
      }

      command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetMaxValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      p1 = Connection.CreateParameter();
      p1.Value = 2;
      _ = command.Parameters.Add(p1);

      p2 = Connection.CreateParameter();
      p2.Value = DateTimeOffset.MaxValue;
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }
    #endregion

    #region Clear structure and data
    private void DropTablesForDateTime()
    {
      var createDateTimeTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeMinValueTable}\";");
      using (createDateTimeTableCommand) {
        _ = createDateTimeTableCommand.ExecuteNonQuery();
      }

      createDateTimeTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeMaxValueTable}\";");
      using (createDateTimeTableCommand) {
        _ = createDateTimeTableCommand.ExecuteNonQuery();
      }
    }

    private void DropTablesForDateOnly()
    {
      var createDateOnlyTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateOnlyMinValueTable}\";");
      using (createDateOnlyTableCommand) {
        _ = createDateOnlyTableCommand.ExecuteNonQuery();
      }

      createDateOnlyTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateOnlyMaxValueTable}\";");
      using (createDateOnlyTableCommand) {
        _ = createDateOnlyTableCommand.ExecuteNonQuery();
      }
    }

    private void DropTablesForDateTimeOffset()
    {
      var createDateTimeOffsetTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeOffsetMinValueTable}\" ;");
      using (createDateTimeOffsetTableCommand) {
        _ = createDateTimeOffsetTableCommand.ExecuteNonQuery();
      }

      createDateTimeOffsetTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeOffsetMaxValueTable}\" ;");
      using (createDateTimeOffsetTableCommand) {
        _ = createDateTimeOffsetTableCommand.ExecuteNonQuery();
      }
    }
    #endregion

    private void CheckIfInfinityAliasTurnedOn()
    {
      if (AppContext.TryGetSwitch(Orm.PostgreSql.WellKnown.DateTimeToInfinityConversionSwitchName, out var flag) && flag) {
        throw new IgnoreException("Require date to Infinity conversion");
      }
    }

    private void CheckIfInfinityAliasTurnedOff()
    {
      if (!AppContext.TryGetSwitch(Orm.PostgreSql.WellKnown.DateTimeToInfinityConversionSwitchName, out var flag) || !flag) {
        throw new IgnoreException("Require no date to Infinity conversion");
      }
    }

  }
}
