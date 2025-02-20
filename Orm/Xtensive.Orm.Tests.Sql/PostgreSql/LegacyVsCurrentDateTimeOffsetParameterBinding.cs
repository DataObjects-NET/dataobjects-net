// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;


namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public sealed class LegacyVsCurrentDateTimeOffsetParameterBinding : SqlTest
  {
    private const string DateTimeOffsetValueTable = "DateTimeOffsetTable";

    private Xtensive.Sql.TypeMapping longTypeMapping;
    private Xtensive.Sql.TypeMapping dateTimeOffsetTypeMapping;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      longTypeMapping = Driver.TypeMappings[typeof(long)];
      dateTimeOffsetTypeMapping = Driver.TypeMappings[typeof(DateTimeOffset)];

      DropTablesForTests();

      CreateTablesForTests();
    }

    [Test]
    public void WriteUtcValueLegacy()
    {
      CheckLegacyTurnedOn();

      var utcValue = DateTimeOffset.UtcNow;

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, utcValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(p2, utcValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    public void WriteLocalValueLegacy()
    {
      CheckLegacyTurnedOn();

      var localKindValue = DateTimeOffset.Now;

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, localKindValue.Ticks);
      p1.DbType = System.Data.DbType.Int64;
      p1.Value = localKindValue.Ticks;
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(p2, localKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }


    [Test]
    public void WriteUtcValue()
    {
      CheckLegacyTurnedOff();

      var utcKindValue = DateTimeOffset.UtcNow;

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, utcKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(p2, utcKindValue);
      _ = command.Parameters.Add(p2);
      using (command) {
        _ = command.ExecuteNonQuery();
      }
    }

    [Test]
    public void WriteLocalValue()
    {
      CheckLegacyTurnedOff();

      var localKindValue = DateTimeOffset.Now;

      var command = Connection.CreateCommand($"INSERT INTO \"{DateTimeOffsetValueTable}\"(\"Id\", \"Value\") VALUES ($1, $2)");
      var p1 = Connection.CreateParameter();
      longTypeMapping.BindValue(p1, localKindValue.Ticks);
      _ = command.Parameters.Add(p1);

      var p2 = Connection.CreateParameter();
      dateTimeOffsetTypeMapping.BindValue(p2, localKindValue);
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
          $"CREATE TABLE IF NOT EXISTS \"{DateTimeOffsetValueTable}\" (\"Id\" bigint CONSTRAINT PK_{DateTimeOffsetValueTable} PRIMARY KEY, \"Value\" timestamp);");
      using (createTableCommand) {
        _ = createTableCommand.ExecuteNonQuery();
      }
    }

    #endregion

    #region Clear structure
    private void DropTablesForTests()
    {
      var dropTableCommand = Connection.CreateCommand($"DROP TABLE IF EXISTS \"{DateTimeOffsetValueTable}\";");
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
