// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
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
    /// Gets system time zone info for server time zone, if such zone exists.
    /// </summary>
    /// <param name="connectionTimezone">Time zone from connection</param>
    /// <returns>Instance of <see cref="TimeZoneInfo"/> if such found, or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Server timezone offset can't be recognized.</exception>
    public static TimeZoneInfo GetTimeZoneInfoForServerTimeZone(string connectionTimezone)
    {
      if (string.IsNullOrEmpty(connectionTimezone)) {
        return null;
      }

      // Try to get zone as is, conversion from IANA format identifier
      // happens inside the TimeZoneInfo.FindSystemTimeZoneById().
      // Postgres uses IANA timezone format identifier, not windows one.
      if (TryFindSystemTimeZoneById(connectionTimezone, out var result)) {
        return result;
      }

      // If zone was set as certain offset, then it will be returned to us in form of 
      // POSIX offset, e.g. '<+03>-03' for UTC+03 or '<+1030>-1030' for UTC+10:30
      if (Regex.IsMatch(connectionTimezone, "^<[+|-]\\d{2,4}>[-|+]\\d{2,4}$")) {
        var closingBracketIndex = connectionTimezone.IndexOf('>');
        var utcOffset = connectionTimezone.Substring(1, closingBracketIndex - 1);

        var utcOffsetString = utcOffset.Length switch {
          3 => utcOffset,
          5 => utcOffset.Insert(3, ":"),
          _ => string.Empty
        };

        //Here, we rely on server validation of zone existance for the offset required by user

        var utcIdentifier = $"UTC{utcOffsetString}";

        if (utcIdentifier.Length == 3)
          throw new ArgumentException($"Server connection time zone '{connectionTimezone}' cannot be recongized.");

        if (TryFindSystemTimeZoneById(utcIdentifier, out var utcTimeZone)) {
          return utcTimeZone;
        }
        else {
          var parsingCulture = CultureInfo.InvariantCulture;
          TimeSpan baseOffset;
          if (utcOffsetString.StartsWith("-")) {
            if (!TimeSpan.TryParseExact(utcOffsetString, "\\-hh\\:mm", parsingCulture, TimeSpanStyles.AssumeNegative, out baseOffset))
              if(!TimeSpan.TryParseExact(utcOffsetString, "\\-hh", parsingCulture, TimeSpanStyles.AssumeNegative, out baseOffset))
                throw new ArgumentException($"Server connection time zone '{connectionTimezone}' cannot be recongized.");
          }
          else {
            if (!TimeSpan.TryParseExact(utcOffsetString, "\\+hh\\:mm", parsingCulture, TimeSpanStyles.None, out baseOffset))
              if (!TimeSpan.TryParseExact(utcOffsetString, "\\+hh", parsingCulture, TimeSpanStyles.None, out baseOffset))
                throw new ArgumentException($"Server connection time zone '{connectionTimezone}' cannot be recongized.");
          }

          return TimeZoneInfo.CreateCustomTimeZone(utcIdentifier, baseOffset, "Coordinated Universal Time" + utcOffsetString, utcIdentifier);
        }
      }

      return null;
    }

    private static bool TryFindSystemTimeZoneById(string id, out TimeZoneInfo timeZoneInfo)
    {
#if NET8_0_OR_GREATER
      return TimeZoneInfo.TryFindSystemTimeZoneById(id, out timeZoneInfo);
#else
      try {
        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id);
        return true;
      }
      catch {
        timeZoneInfo = null;
        return false;
      }
#endif
    }
  }
}
