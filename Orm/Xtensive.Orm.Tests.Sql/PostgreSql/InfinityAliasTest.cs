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
      var template = templates[DateTimeMinValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Year, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using(command)
      using(var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Year, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Month, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Day, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Hour, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Hour, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Hour, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Minute, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Minute, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Minute, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Second, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Second, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MinValue.Second, isOn);
        }
      }
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
      var template = templates[DateTimeMaxValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Year, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Year, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Month, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Day, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Hour, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Hour, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Hour, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Minute, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Minute, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Minute, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Second, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Second, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTime.MaxValue.Second, isOn);
        }
      }
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
      var template = templates[DateOnlyMinValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Year, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Year, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Month, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateOnlyMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MinValue.Day, isOn);
        }
      }
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
      var template = templates[DateOnlyMaxValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Year, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Year, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Month, isOn);
        }
      }


      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateOnlyMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDatePart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateOnly.MaxValue.Day, isOn);
        }
      }
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
      var template = templates[DateTimeOffsetMinValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTimeOffset.MinValue.Year, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTimeOffset.MinValue.Year, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTimeOffset.MinValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPart(partValue, DateTimeOffset.MinValue.Month, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPartNative(partValue, DateTimeOffset.MinValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          CheckPart(partValue, DateTimeOffset.MinValue.Day, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, 4, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPart(partValue, (isOn) ? DateTimeOffset.MinValue.Hour : 4, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, 2, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPart(partValue, (isOn) ? DateTimeOffset.MinValue.Minute : 2, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeOffsetMinValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, 33, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Second, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MinValue value in postgre is set to 04:02:33, at least when instance is in UTC+5 timezone
          CheckPart(partValue, (isOn) ? DateTimeOffset.MinValue.Second : 33, isOn);
        }
      }
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
      var template = templates[DateTimeOffsetMaxValueTable];

      var command = Connection.CreateCommand($"SELECT EXTRACT (YEAR FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // There is overflow of year because of PostgreSQL time zone functionality
          CheckPartNative(partValue, DateTimeOffset.MaxValue.Year + 1, isOn);
        }
      }

      var select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Year, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // There is overflow of year because of PostgreSQL time zone functionality
          CheckPart(partValue, (isOn) ? DateTimeOffset.MaxValue.Year : DateTimeOffset.MaxValue.Year + 1, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MONTH FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // there is value overflow to 01
          CheckPartNative(partValue, DateTimeOffset.MinValue.Month, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Month, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // there is value overflow to 01 in case of no aliases
          CheckPart(partValue, (isOn) ? DateTimeOffset.MaxValue.Month : DateTimeOffset.MinValue.Month, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (DAY FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // there is value overflow to 01
          CheckPartNative(partValue, DateTime.MinValue.Day, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Day, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // there is value overflow to 01 in case of no aliases
          CheckPart(partValue, (isOn) ? DateTimeOffset.MaxValue.Day : DateTimeOffset.MinValue.Day, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (HOUR FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, 4, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPart(partValue, (isOn) ? DateTimeOffset.MaxValue.Hour : 4, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (MINUTE FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, 59, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPart(partValue, DateTimeOffset.MaxValue.Minute, isOn);
        }
      }

      command = Connection.CreateCommand($"SELECT EXTRACT (SECOND FROM \"Value\") FROM public.\"{DateTimeOffsetMaxValueTable}\"");
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPartNative(partValue, DateTimeOffset.MaxValue.Second, isOn);
        }
      }

      select = template.Clone(new SqlNodeCloneContext());
      select.Columns.Add(SqlDml.Extract(SqlDateTimeOffsetPart.Second, select.From.Columns["Value"]));

      command = Connection.CreateCommand(select);
      using (command)
      using (var reader = command.ExecuteReader()) {

        while (reader.Read()) {
          var partValue = reader.GetDouble(0);
          // timezone for DateTimeOffset.MaxValue value in postgre is set to 04:59:59.999999, at least when instance is in UTC+5 timezone
          CheckPart(partValue, DateTimeOffset.MaxValue.Second, isOn);
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
