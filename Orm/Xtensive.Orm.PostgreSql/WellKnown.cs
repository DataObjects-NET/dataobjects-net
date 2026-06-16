// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.PostgreSql
{
  internal static class WellKnown
  {
    public const string DateTimeToInfinityConversionSwitchName = "Npgsql.DisableDateTimeInfinityConversions";
    public const string LegacyTimestampBehaviorSwitchName = "Npgsql.EnableLegacyTimestampBehavior";

    public const int IntervalDaysInMonth = 30;
  }
}
