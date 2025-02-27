// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using Xtensive.Orm.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  internal static class PostgreSqlHelper
  {
    internal static NpgsqlInterval CreateNativeIntervalFromTimeSpan(in TimeSpan timeSpan)
    {
      // Previous Npgsql versions used days and time, no months.
      // Thought we can write everything as time, we keep days and time format

      var ticks = timeSpan.Ticks;

      var days = timeSpan.Days;
      var timeTicks = ticks - (days * TimeSpan.TicksPerDay);
#if NET7_0_OR_GREATER
      var microseconds = timeTicks / TimeSpan.TicksPerMicrosecond;
#else
      var microseconds = timeTicks / 10L; // same as TimeSpan.TicksPerMicrosecond available in .NET7+
#endif
      // no months!
      return new NpgsqlInterval(0, days, microseconds);
    }

    internal static TimeSpan ResurrectTimeSpanFromNpgsqlInterval(in NpgsqlInterval npgsqlInterval)
    {
      // We don't write "Months" part of NpgsqlInterval to database
      // because days in months is variable measure in PostgreSQL.
      // We better use exact number of days.
      // But if for some reason, there is Months value > 0 we treat it like each month has 30 days,
      // it seems that Npgsql did the same assumption internally.

      var days = (npgsqlInterval.Months != 0)
        ? npgsqlInterval.Months * WellKnown.IntervalDaysInMonth + npgsqlInterval.Days
        : npgsqlInterval.Days;

      var ticksOfDays = days * TimeSpan.TicksPerDay;
#if NET7_0_OR_GREATER
      var overallTicks = ticksOfDays + (npgsqlInterval.Time * TimeSpan.TicksPerMicrosecond);
#else
      var overallTicks = ticksOfDays + (npgsqlInterval.Time * 10); //same as TimeSpan.TicksPerMicrosecond available in .NET7+
#endif
      return TimeSpan.FromTicks(overallTicks);
    }

    /// <summary>
    /// Checks if timezone is declared in POSIX format (example  &lt;+07&gt;-07 )
    /// and returns number between '&lt;' and '&gt;' as timezone.
    /// </summary>
    /// <param name="timezone">Timezone in possible POSIX format</param>
    /// <returns>Timezone shift declared in oritinal POSIX format as timezone or original value.</returns>
    internal static string TryGetZoneFromPosix(string timezone)
    {
      if (timezone.StartsWith('<')) {
        // if POSIX format 
        var closing = timezone.IndexOf('>');
        var result =  timezone.Substring(1, closing - 1);
        return result;
      }
      return timezone;
    }
  }
}
