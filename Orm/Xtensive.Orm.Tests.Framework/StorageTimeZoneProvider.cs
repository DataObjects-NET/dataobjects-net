// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests
{
  internal abstract class StorageTimeZoneProvider : IStorageTimeZoneProvider
  {
    public TimeSpan TimeZoneOffset { get; private set; }

    protected abstract TimeSpan GetServerTimezone(SqlDriver sqlDriver);

    public StorageTimeZoneProvider(SqlDriver sqlDriver)
    {
      TimeZoneOffset = GetServerTimezone(sqlDriver);
    }
  }

  internal sealed class LocalTimeZoneProvider : IStorageTimeZoneProvider
  {
    public TimeSpan TimeZoneOffset => TimeZoneInfo.Local.BaseUtcOffset;
  }

  internal sealed class SqlServerTimeZoneProvider : StorageTimeZoneProvider
  {
    protected override TimeSpan GetServerTimezone(SqlDriver sqlDriver)
    {
      using (var c = sqlDriver.CreateConnection()) {
        c.Open();
        using (var cmd = c.CreateCommand("SELECT SYSDATETIMEOFFSET();")) {
          var dateTimeOffset = (DateTimeOffset) cmd.ExecuteScalar();
          return dateTimeOffset.Offset;
        }
      }
    }

    public SqlServerTimeZoneProvider(SqlDriver sqlDriver)
      : base(sqlDriver)
    {
    }
  }

  internal sealed class OracleTimeZoneProvider : StorageTimeZoneProvider
  {
    protected override TimeSpan GetServerTimezone(SqlDriver sqlDriver)
    {
      var mappings = sqlDriver.TypeMappings[typeof(DateTimeOffset)];

      TimeSpan value;
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();
        var commandText = "select CAST(systimestamp AS timestamp with time zone) as \"local_time\" from dual";
        using (var cmd = connection.CreateCommand(commandText)) {
          using (var reader = cmd.ExecuteReader()) {
            _ = reader.Read();
            var dateTimeOffset = (DateTimeOffset) mappings.ReadValue(reader, 0);
            value = dateTimeOffset.Offset;
          }
        }
        connection.Close();
      }
      return value;
    }

    public OracleTimeZoneProvider(SqlDriver sqlDriver)
      : base(sqlDriver)
    {
    }
  }

  internal sealed class PgSqlTimeZoneProvider : StorageTimeZoneProvider
  {
    protected override TimeSpan GetServerTimezone(SqlDriver sqlDriver)
    {
      var mappings = sqlDriver.TypeMappings[typeof(DateTimeOffset)];
      var referenceDate = new DateTime(2016, 10, 23);
      TimeSpan value;
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();
        var commandText = $"select CAST('{referenceDate.ToString("yyyy-MM-dd HH:mm:ss")}' AS timestamp with time zone) as \"local_time\"";
        using (var cmd = connection.CreateCommand(commandText)) {
          using (var reader = cmd.ExecuteReader()) {
            _ = reader.Read();
            var dateTimeOffset = (DateTimeOffset) mappings.ReadValue(reader, 0);

            value = dateTimeOffset.Offset;
            if (dateTimeOffset.DateTime != referenceDate) {
              value = value.Add(referenceDate - dateTimeOffset.DateTime);
            }
          }
        }
        connection.Close();
      }
      return value;
    }

    public PgSqlTimeZoneProvider(SqlDriver sqlDriver) : base(sqlDriver)
    {
    }
  }

  internal sealed class SqliteTimeZoneProvider : StorageTimeZoneProvider
  {
    protected override TimeSpan GetServerTimezone(SqlDriver sqlDriver)
    {
      TimeSpan value;
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();
        var commandText = "SELECT ((STRFTIME ('%s', '2016-01-01 12:00:00') - STRFTIME ('%s', '2016-01-01 12:00:00', 'UTC')) / 60)";
        using (var cmd = connection.CreateCommand(commandText)) {
          using (var reader = cmd.ExecuteReader()) {
            _ = reader.Read();
            var offsetInMinutes = reader.GetInt32(0);

            value = new TimeSpan(offsetInMinutes / 60, offsetInMinutes % 60, 0);
          }
        }
        connection.Close();
      }
      return value;
    }

    public SqliteTimeZoneProvider(SqlDriver sqlDriver)
      : base(sqlDriver)
    {
    }
  }
}