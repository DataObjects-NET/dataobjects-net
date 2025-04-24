using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Sql.Drivers.PostgreSql;
using PostgreSqlDriver = Xtensive.Sql.Drivers.PostgreSql.Driver;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [TestFixture]
  public sealed class PostgreSqlHelperTest : SqlTest
  {
    private string[] timezoneIdsWithWinAnalogue;
    private string[] timezoneIdsWithoutWinAnalogue;

    public static TimeSpan[] Intervals
    {
      get => new[] {
        TimeSpan.FromDays(66).Add(TimeSpan.FromHours(4)).Add(TimeSpan.FromMinutes(45)).Add(TimeSpan.FromSeconds(36)),
        TimeSpan.FromDays(32).Add(TimeSpan.FromHours(2)).Add(TimeSpan.FromMinutes(44)).Add(TimeSpan.FromSeconds(35)),
        TimeSpan.FromDays(16).Add(TimeSpan.FromHours(3)).Add(TimeSpan.FromMinutes(43)).Add(TimeSpan.FromSeconds(34)),
        TimeSpan.FromDays(3).Add(TimeSpan.FromHours(1)).Add(TimeSpan.FromMinutes(42)).Add(TimeSpan.FromSeconds(33)),
        TimeSpan.FromHours(25).Add(TimeSpan.FromMinutes(15)).Add(TimeSpan.FromSeconds(44)),
        TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(14)).Add(TimeSpan.FromSeconds(43)),
        TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(13)).Add(TimeSpan.FromSeconds(42)),
        TimeSpan.FromHours(4).Add(TimeSpan.FromMinutes(12)).Add(TimeSpan.FromSeconds(41)),
        TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(11)).Add(TimeSpan.FromSeconds(40)),
        TimeSpan.FromMinutes(65).Add(TimeSpan.FromSeconds(48)),
        TimeSpan.FromMinutes(59).Add(TimeSpan.FromSeconds(47)),
        TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(46)),
        TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(45)),
        TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(44)),
        TimeSpan.FromSeconds(44).Add(TimeSpan.FromMilliseconds(445)),
        TimeSpan.FromSeconds(44).Add(TimeSpan.FromMilliseconds(400)),
        TimeSpan.FromSeconds(44).Add(TimeSpan.FromMilliseconds(332)),
        TimeSpan.FromSeconds(44).Add(TimeSpan.FromMilliseconds(248)),
        TimeSpan.FromSeconds(44).Add(TimeSpan.FromMilliseconds(183)),
        TimeSpan.FromMilliseconds(444),
        TimeSpan.FromMilliseconds(402),
        TimeSpan.FromMilliseconds(333),
        TimeSpan.FromMilliseconds(249),
        TimeSpan.FromMilliseconds(181)
      };
    }

    public static string[] PosixOffsetFormatValues
    {
      get => new[] {
        "<+02>-02",
        "<+05>-05",
        "<+07>-07",
        "<-02>+02",
        "<-05>+05",
        "<-07>+07",
        "<-0730>+0730"
      };
    }

    public static string[] PseudoPosixOffsetFormatValues
    {
      get => new[] {
        "<+2>-2",
        "<+5>-5",
        "<+7>-7",
        "<-2>+2",
        "<-5>+5",
        "<-7>+7",
        "<ulalala>not-ulalala"
      };
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      LoadServerTimeZones(Connection, out timezoneIdsWithWinAnalogue, out timezoneIdsWithoutWinAnalogue);

      Connection.Close();
    }

    [Test]
    [TestCaseSource(nameof(PosixOffsetFormatValues))]
    public void PosixOffsetRecognitionTest(string offset)
    {
      var systemTimezone = PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(offset);
      Assert.That(systemTimezone, Is.Not.Null);
      Assert.That(systemTimezone.Id.Contains("UTC"));
    }

    [Test]
    [TestCaseSource(nameof(PseudoPosixOffsetFormatValues))]
    public void PseudoPosixOffsetRecognitionTest(string offset)
    {
      var systemTimezone = PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(offset);
      Assert.That(systemTimezone, Is.Null);
    }

    [Test]
    public void ResolvableTimeZonesTest()
    {
      foreach (var tz in timezoneIdsWithWinAnalogue) {
        Assert.That(PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(tz), Is.Not.Null, tz);
      }
    }

    [Test]
    public void UnresolvableTimeZonesTest()
    {
      foreach(var tz in timezoneIdsWithoutWinAnalogue) {
        Assert.That(PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(tz), Is.Null, tz);
      }
    }

    [Test]
    [TestCaseSource(nameof(Intervals))]
    public void TimeSpanToIntervalConversionTest(TimeSpan testValue)
    {
      var nativeInterval = PostgreSqlHelper.CreateNativeIntervalFromTimeSpan(testValue);
      var backToTimeSpan = PostgreSqlHelper.ResurrectTimeSpanFromNpgsqlInterval(nativeInterval);
      Assert.That(backToTimeSpan, Is.EqualTo(testValue));
    }


    private static void LoadServerTimeZones(Xtensive.Sql.SqlConnection connection,
      out string[] timezoneIdsWithWinAnalogue,
      out string[] timezoneIdsWithoutWinAnalogue)
    {
      var timezoneIdsWithWinAnalogueList = new List<string>();
      var timezoneIdsWithoutWinAnalogueList = new List<string>();

      var existing = new HashSet<string>();
      var serverTimeZoneAbbrevs = new HashSet<string>();
      using (var command = connection.CreateCommand("SELECT \"name\", \"abbrev\" FROM pg_catalog.pg_timezone_names"))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var name = reader.GetString(0);
          var abbrev = reader.GetString(1);

          if (TryFindSystemTimeZoneById(name, out var winAnalogue))
            timezoneIdsWithWinAnalogueList.Add(name);
          else
            timezoneIdsWithoutWinAnalogueList.Add(name);

          if (abbrev[0] != '-' && abbrev[0] != '+' && existing.Add(abbrev)) {
            if (TryFindSystemTimeZoneById(abbrev, out var winAnalogue1))
              timezoneIdsWithWinAnalogueList.Add(abbrev);
            else
              timezoneIdsWithoutWinAnalogueList.Add(abbrev);
          }
        }
      }

      using (var command = connection.CreateCommand("SELECT \"abbrev\" FROM pg_catalog.pg_timezone_abbrevs"))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var abbrev = reader.GetString(0);

          if (TryFindSystemTimeZoneById(abbrev, out var winAnalogue))
            timezoneIdsWithWinAnalogueList.Add(abbrev);
          else
            timezoneIdsWithoutWinAnalogueList.Add(abbrev);

          if (existing.Add(abbrev)) {
            if (TryFindSystemTimeZoneById(abbrev, out var winAnalogue1))
              timezoneIdsWithWinAnalogueList.Add(abbrev);
            else
              timezoneIdsWithoutWinAnalogueList.Add(abbrev);
          }
        }
      }
      timezoneIdsWithoutWinAnalogue = timezoneIdsWithoutWinAnalogueList.ToArray();
      timezoneIdsWithWinAnalogue = timezoneIdsWithWinAnalogueList.ToArray();
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
