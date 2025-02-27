// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [TestFixture]
  public sealed class InfinityAliasTest : SqlTest
  {
    private const string DateOnlyMinValueTable = "DateOnlyTable1";
    private const string DateOnlyMaxValueTable = "DateOnlyTable2";

    private const string DateTimeMinValueTable = "DateTimeTable1";
    private const string DateTimeMaxValueTable = "DateTimeTable2";

    private const string DateTimeOffsetMinValueTable = "DateTimeOffsetTable1";
    private const string DateTimeOffsetMaxValueTable = "DateTimeOffsetTable2";

    private readonly Dictionary<string, SqlSelect> templates = new();

    private TypeMapping longTypeMapping;
    private TypeMapping dateOnlyTypeMapping;
    private TypeMapping dateTimeTypeMapping;
    private TypeMapping dateTimeOffsetTypeMapping;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      var localZone = DateTimeOffset.Now.ToLocalTime().Offset;
      var localZoneString = ((localZone < TimeSpan.Zero) ? "-" : "+") + localZone.ToString(@"hh\:mm");
      var initConnectionCommand = Connection.CreateCommand($"SET TIME ZONE INTERVAL '{localZoneString}' HOUR TO MINUTE");
      _ = initConnectionCommand.ExecuteNonQuery();

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

      var schema = ExtractDefaultSchema();

      templates.Add(DateOnlyMinValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateOnlyMinValueTable])));

      templates.Add(DateOnlyMaxValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateOnlyMaxValueTable])));


      templates.Add(DateTimeMinValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateTimeMinValueTable])));

      templates.Add(DateTimeMaxValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateTimeMaxValueTable])));


      templates.Add(DateTimeOffsetMinValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateTimeOffsetMinValueTable])));

      templates.Add(DateTimeOffsetMaxValueTable,
        SqlDml.Select(SqlDml.TableRef(schema.Tables[DateTimeOffsetMaxValueTable])));
    }

    [Test]
    public void DateTimeMinSelectNoFilterTest()
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

      var select = templates[DateTimeMinValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var datetimeValue = dateTimeTypeMapping.ReadValue(reader, 1);
          Assert.That(datetimeValue, Is.EqualTo(DateTime.MinValue));
        }
      }
    }

    [Test]
    public void DateTimeMinSelectByEqualityTest()
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
    public void DateTimeMinSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateTimeSelectDatePart(true);
    }

    [Test]
    public void DateTimeMinSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateTimeSelectDatePart(false);
    }

    private void TestMinDateTimeSelectDatePart(bool isOn)
    {
      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Year,
        DateTime.MinValue.Year, DateTime.MinValue.Year, isOn);

      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Month,
        DateTime.MinValue.Month, DateTime.MinValue.Month, isOn);

      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Day,
        DateTime.MinValue.Day, DateTime.MinValue.Day, isOn);

      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Hour,
        DateTime.MinValue.Hour, DateTime.MinValue.Hour, isOn);

      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Minute,
        DateTime.MinValue.Minute, DateTime.MinValue.Minute, isOn);

      TestDateTimePartExtraction(DateTimeMinValueTable, SqlDateTimePart.Second,
        DateTime.MinValue.Second, DateTime.MinValue.Second, isOn);
    }


    [Test]
    public void DateTimeMaxSelectNoFilterTest()
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

      var select = templates[DateTimeMaxValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var datetimeValue = (DateTime) dateTimeTypeMapping.ReadValue(reader, 1);
          var difference = (datetimeValue - DateTime.MaxValue).Duration();
          Assert.That(difference, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(0.001)));
        }
      }
    }

    [Test]
    public void DateTimeMaxSelectByEqualityTest()
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
    public void DateTimeMaxSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateTimeSelectDatePart(true);
    }

    [Test]
    public void DateTimeMaxSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateTimeSelectDatePart(false);
    }

    private void TestMaxDateTimeSelectDatePart(bool isOn)
    {
      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Year,
        DateTime.MaxValue.Year, DateTime.MaxValue.Year, isOn);

      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Month,
        DateTime.MaxValue.Month, DateTime.MaxValue.Month, isOn);

      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Day,
        DateTime.MaxValue.Day, DateTime.MaxValue.Day, isOn);

      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Hour,
        DateTime.MaxValue.Hour, DateTime.MaxValue.Hour, isOn);

      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Minute,
        DateTime.MaxValue.Minute, DateTime.MaxValue.Minute, isOn);

      TestDateTimePartExtraction(DateTimeMaxValueTable, SqlDateTimePart.Second,
        DateTime.MaxValue.Second, DateTime.MaxValue.Second, isOn);
    }


    [Test]
    public void DateOnlyMinNoFilterTest()
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

      var select = templates[DateOnlyMinValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var datetimeValue = dateOnlyTypeMapping.ReadValue(reader, 1);
          Assert.That(datetimeValue, Is.EqualTo(DateOnly.MinValue));
        }
      }
    }

    [Test]
    public void DateOnlyMinByEqualityTest()
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
    public void DateOnlyMinSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateOnlySelectDatePart(true);
    }

    [Test]
    public void DateOnlyMinSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateOnlySelectDatePart(false);
    }

    private void TestMinDateOnlySelectDatePart(bool isOn)
    {
      TestDatePartExtraction(DateOnlyMinValueTable, SqlDatePart.Year,
         DateOnly.MinValue.Year, DateOnly.MinValue.Year, isOn);

      TestDatePartExtraction(DateOnlyMinValueTable, SqlDatePart.Month,
         DateOnly.MinValue.Month, DateOnly.MinValue.Month, isOn);

      TestDatePartExtraction(DateOnlyMinValueTable, SqlDatePart.Day,
         DateOnly.MinValue.Day, DateOnly.MinValue.Day, isOn);
    }


    [Test]
    public void DateOnlyMaxNoFilterTest()
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

      var select = templates[DateTimeMaxValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var datetimeValue = dateOnlyTypeMapping.ReadValue(reader, 1);
          Assert.That(datetimeValue, Is.EqualTo(DateOnly.MaxValue));
        }
      }
    }

    [Test]
    public void DateOnlyMaxByEqualityTest()
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
    public void DateOnlyMaxSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateOnlySelectDatePart(true);
    }

    [Test]
    public void DateOnlyMaxSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateOnlySelectDatePart(false);
    }

    private void TestMaxDateOnlySelectDatePart(bool isOn)
    {
      TestDatePartExtraction(DateOnlyMaxValueTable, SqlDatePart.Year,
         DateOnly.MaxValue.Year, DateOnly.MaxValue.Year, isOn);

      TestDatePartExtraction(DateOnlyMaxValueTable, SqlDatePart.Month,
         DateOnly.MaxValue.Month, DateOnly.MaxValue.Month, isOn);

      TestDatePartExtraction(DateOnlyMaxValueTable, SqlDatePart.Day,
         DateOnly.MaxValue.Day, DateOnly.MaxValue.Day, isOn);
    }


    [Test]
    public void DateTimeOffsetMinSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var dateTimeOffsetValue = (DateTimeOffset) reader.GetFieldValue<DateTimeOffset>(1);
          Assert.That(dateTimeOffsetValue, Is.EqualTo(DateTimeOffset.MinValue));
        }
      }

      var select = templates[DateTimeOffsetMinValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var dateTimeOffsetValue = dateTimeOffsetTypeMapping.ReadValue(reader, 1);
          Assert.That(dateTimeOffsetValue, Is.EqualTo(DateTimeOffset.MinValue));
        }
      }
    }

    [Test]
    public void DateTimeOffsetMinSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeOffsetMinValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(filterP, DateTimeOffset.MinValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void DateTimeOffsetMinSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMinDateTimeOffsetSelectDatePart(true);
    }

    [Test]
    public void DateTimeOffsetMinSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMinDateTimeOffsetSelectDatePart(false);
    }

    private void TestMinDateTimeOffsetSelectDatePart(bool isOn)
    {
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Year,
        DateTimeOffset.MinValue.Year,
        DateTimeOffset.MinValue.Year,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Month,
        DateTimeOffset.MinValue.Month,
        DateTimeOffset.MinValue.Month,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Day,
        DateTimeOffset.MinValue.Day,
        DateTimeOffset.MinValue.Day,
        isOn);

      // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Hour,
        5,
        isOn ? DateTimeOffset.MinValue.Hour : 5,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Minute,
        DateTimeOffset.MinValue.Minute,
        DateTimeOffset.MinValue.Minute,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMinValueTable, SqlDateTimeOffsetPart.Second,
        DateTimeOffset.MinValue.Second,
        DateTimeOffset.MinValue.Second,
        isOn);
    }


    [Test]
    public void DateTimeOffsetMaxSelectNoFilterTest()
    {
      var command = Connection.CreateCommand($"SELECT \"Id\", \"Value\" FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = reader.GetInt64(0);
          var dateTimeOffsetValue = (DateTimeOffset) reader.GetFieldValue<DateTimeOffset>(1);
          var difference = (dateTimeOffsetValue - DateTimeOffset.MaxValue).Duration();
          Assert.That(difference, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(0.001)));
        }
      }

      var select = templates[DateTimeOffsetMaxValueTable].Clone(new SqlNodeCloneContext());
      select.Columns.Add(select.From.Columns["Id"]);
      select.Columns.Add(select.From.Columns["Value"]);

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var id = longTypeMapping.ReadValue(reader, 0);
          var dateTimeOffsetValue = (DateTimeOffset) dateTimeOffsetTypeMapping.ReadValue(reader, 1);
          var difference = (dateTimeOffsetValue - DateTimeOffset.MaxValue).Duration();
          Assert.That(difference, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(0.001)));
        }
      }
    }

    [Test]
    public void DateTimeOffsetMaxSelectByEqualityTest()
    {
      var command = Connection.CreateCommand($"SELECT Count(*) FROM public.\"{DateTimeOffsetMaxValueTable}\" WHERE \"Value\" = $1");
      var filterP = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(filterP, DateTimeOffset.MaxValue);
      _ = command.Parameters.Add(filterP);

      using (command) {
        var count = (long) command.ExecuteScalar();
        Assert.That(count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void DateTimeOffsetMaxSelectDatePartInfinityTest()
    {
      CheckIfInfinityAliasTurnedOn();

      TestMaxDateTimeOffsetSelectDatePart(true);
    }

    [Test]
    public void DateTimeOffsetMaxSelectDatePartDateTest()
    {
      CheckIfInfinityAliasTurnedOff();

      TestMaxDateTimeOffsetSelectDatePart(false);
    }

    private void TestMaxDateTimeOffsetSelectDatePart(bool isOn)
    {
      // There is overflow of year because of PostgreSQL time zone functionality
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Year,
        DateTimeOffset.MaxValue.Year + 1,
        (isOn) ? DateTimeOffset.MaxValue.Year : DateTimeOffset.MaxValue.Year + 1,
        isOn);

      // there is value overflow to 01 in case of no aliases
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Month,
        1,
        (isOn) ? DateTimeOffset.MaxValue.Month : 1,
        isOn);
      // there is value overflow to 01 in case of no aliases
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Day,
        1,
        (isOn) ? DateTimeOffset.MaxValue.Day : 1,
        isOn);

      // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Hour,
        4,
        (isOn) ? DateTimeOffset.MaxValue.Hour : 4,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Minute,
        DateTimeOffset.MaxValue.Minute,
        DateTimeOffset.MaxValue.Minute,
        isOn);
      TestDateTimeOffsetPartExtraction(DateTimeOffsetMaxValueTable, SqlDateTimeOffsetPart.Second,
        DateTimeOffset.MaxValue.Second,
        DateTimeOffset.MaxValue.Second,
        isOn);
    }

    private void TestDatePartExtraction(string table, SqlDatePart part, int expectedValueNative, int expectedValueDml, bool aliasesEnabled)
    {
      var template = templates[table];

      var command = Connection.CreateCommand($"SELECT EXTRACT ({part.ToString().ToUpperInvariant()} FROM \"Value\") FROM public.\"{table}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, expectedValueNative, aliasesEnabled);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(part, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPart(partValue, expectedValueDml, aliasesEnabled);
        }
      }
    }

    private void TestDateTimePartExtraction(string table, SqlDateTimePart part, int expectedValueNative, int expectedValueDml, bool aliasesEnabled)
    {
      var template = templates[table];

      var command = Connection.CreateCommand($"SELECT EXTRACT ({part.ToString().ToUpperInvariant()} FROM \"Value\") FROM public.\"{table}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, expectedValueNative, aliasesEnabled);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(part, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPart(partValue, expectedValueDml, aliasesEnabled);
        }
      }
    }

    private void TestDateTimeOffsetPartExtraction(string table, SqlDateTimeOffsetPart part, int expectedValueNative, int expectedValueDml, bool aliasesEnabled)
    {
      var template = templates[table];

      var command = Connection.CreateCommand($"SELECT EXTRACT ({part.ToString().ToUpperInvariant()} FROM \"Value\") FROM public.\"{table}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, expectedValueNative, aliasesEnabled);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(part, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPart(partValue, expectedValueDml, aliasesEnabled);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (TIMEZONE FROM \"Value\") FROM public.\"{table}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          Console.WriteLine($"TIMEZONE: {partValue}");
        }
      }
    }


    private void CheckPartNative(double partValue, int refPartValue, bool aliasesOn)
    {
      if (aliasesOn) {
        Assert.That(partValue, Is.EqualTo(double.PositiveInfinity).Or.EqualTo(double.NegativeInfinity));
      }
      else {
        Assert.That((int) partValue, Is.EqualTo(refPartValue));
      }
    }

    private void CheckPart(double partValue, int refPartValue, bool aliasesOn)
    {
      if (aliasesOn) {
        Assert.That(partValue, Is.Not.EqualTo(double.PositiveInfinity).And.Not.EqualTo(double.NegativeInfinity));
        Assert.That(partValue, Is.EqualTo(refPartValue));
      }
      else {
        Assert.That((int) partValue, Is.EqualTo(refPartValue));
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
