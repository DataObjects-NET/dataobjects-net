// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;


namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public sealed class LegacyVsCurrentDateTimeParameterBinding : SqlTest
  {
    private const string DateTimeValueTable = "DateTimeTable";

    private const string DateTimeOffsetValueTable = "DateTimeOffsetTable1";

    private Xtensive.Sql.TypeMapping longTypeMapping;
    private Xtensive.Sql.TypeMapping dateTimeTypeMapping;

    protected override void CheckRequirements()
    {
      // do not check provider here.
      // Require class uses driver creation which casues AppContext switch setup before TestFixtureSetup() method called
    }

    protected override void TestFixtureSetUp()
    {
      // use one or enother
      EnableLegacyTimestampBehavior();
      // or
      //DisableLegacyTimestampBehavior();

      Require.ProviderIs(StorageProvider.PostgreSql);

      base.TestFixtureSetUp();

      longTypeMapping = Driver.TypeMappings[typeof(long)];
      dateTimeTypeMapping = Driver.TypeMappings[typeof(DateTime)];

      DropTablesForTests();

      CreateTablesForTests();
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteUtcKindDateTimeValueLegacy()
    {
      CheckLegacyTurnedOn();

      var utcKindValue = DateTime.UtcNow;
      Assert.That(utcKindValue.Kind, Is.EqualTo(DateTimeKind.Utc));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, utcKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, utcKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteLocalKindDateTimeValueLegacy()
    {
      CheckLegacyTurnedOn();

      var localKindValue = DateTime.Now;
      Assert.That(localKindValue.Kind, Is.EqualTo(DateTimeKind.Local));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, localKindValue.Ticks);
      p1.DbType = System.Data.DbType.Int64;
      p1.Value = localKindValue.Ticks;
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, localKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteUnspecifiedKindDateTimeValueLegacy()
    {
      CheckLegacyTurnedOn();

      var unspecifiedKindValue = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
      Assert.That(unspecifiedKindValue.Kind, Is.EqualTo(DateTimeKind.Unspecified));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, unspecifiedKindValue.Ticks);
      p1.DbType = System.Data.DbType.Int64;
      p1.Value = unspecifiedKindValue.Ticks;
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, unspecifiedKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }


    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteUtcKindDateTimeValue()
    {
      CheckLegacyTurnedOff();

      var utcKindValue = DateTime.UtcNow;
      Assert.That(utcKindValue.Kind, Is.EqualTo(DateTimeKind.Utc));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, utcKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, utcKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteLocalKindDateTimeValue()
    {
      CheckLegacyTurnedOff();

      var localKindValue = DateTime.Now;
      Assert.That(localKindValue.Kind, Is.EqualTo(DateTimeKind.Local));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, localKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, localKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    [Explicit("Require manual set of AppContext switch")]
    public void WriteUnspecifiedKindDateTimeValue()
    {
      CheckLegacyTurnedOff();

      var unspecifiedKindValue = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
      Assert.That(unspecifiedKindValue.Kind, Is.EqualTo(DateTimeKind.Unspecified));

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, unspecifiedKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeTypeMapping.BindValue(p2, unspecifiedKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    #region Create structure
    private void CreateTablesForTests()
    {
      var createTableCommand = Connection
        .CreateCommand(
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeValueTable} PRIMARY KEY, \"Value\" timestamp);");
      using (createTableCommand) {
        _ = createTableCommand.ExecuteNonQuery();
      }
    }

    #endregion

    #region Clear structure
    private void DropTablesForTests()
    {
      var dropTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeValueTable}\";");
      using (dropTableCommand) {
        _ = dropTableCommand.ExecuteNonQuery();
      }
    }
    #endregion

    private void EnableLegacyTimestampBehavior()
    {
      AppContext.SetSwitch(Orm.PostgreSql.WellKnown.LegacyTimestampBehaviorSwitchName, true);
    }

    private void DisableLegacyTimestampBehavior()
    {
      AppContext.SetSwitch(Orm.PostgreSql.WellKnown.LegacyTimestampBehaviorSwitchName, false);
    }


    private void CheckLegacyTurnedOn()
    {
      if (AppContext.TryGetSwitch(Orm.PostgreSql.WellKnown.LegacyTimestampBehaviorSwitchName, out var flag) && !flag) {
        throw new IgnoreException("Requires Legacy timestamp behavior");
      }
    }

    private void CheckLegacyTurnedOff()
    {
      if (!AppContext.TryGetSwitch(Orm.PostgreSql.WellKnown.LegacyTimestampBehaviorSwitchName, out var flag) || flag) {
        throw new IgnoreException("Requires no Legacy timestamp behavior");
      }
    }
  }
}