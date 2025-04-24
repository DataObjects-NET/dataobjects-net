// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  /// <summary>
  /// Contains PostgreSQL specific information for driver, including
  /// special settings of Npgsql library which may need for driver.
  /// </summary>
  internal sealed class PostgreServerInfo
  {
    /// <summary>
    /// Indicates whether DateTime.MinValue/MaxValue, DateTimeOffset.MinValue/MaxValue and DateOnly.MinValue/MaxValue,
    /// are replaced with -Infinity/Infinity values inside Npgsql library.
    /// By default replacement is disabled as long as it is allowed by Npgsql.
    /// </summary>
    public bool InfinityAliasForDatesEnabled { get; init; }

    /// <summary>
    /// Indicates whether legacy behavior of timestamp(tz) type inside Npgsql library is enabled.
    /// The setting has effect on parameter binding and also value reading from DbDataReader.
    /// </summary>
    public bool LegacyTimestampBehavior { get; init; }

    /// <summary>
    /// Gets the <see cref="TimeZoneInfo"/> to which <see cref="DateTimeOffset"/> values
    /// will be converted on reading from database.
    /// <see langword="null"/> if no local equivalent of server time zone.
    /// </summary>
    public TimeZoneInfo DefaultTimeZone { get; init; }
  }
}