using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NUnit.Framework;
using Xtensive.Sql.Drivers.PostgreSql;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [TestFixture]
  public sealed class PostgreSqlHelperTest : SqlTest
  {
    private IReadOnlyDictionary<string, TimeSpan> serverTimeZones;

    private string[] testTimezones;

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

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      var nativeDriver = (Xtensive.Sql.Drivers.PostgreSql.Driver) Driver;

      serverTimeZones = nativeDriver.PostgreServerInfo.ServerTimeZones;
      testTimezones = serverTimeZones.Keys.Union(new[] {
        "<+02>-02",
        "<+2>-2",
        "<+05>-05",
        "<+5>-5",
        "<+07>-07",
        "<+7>-7",
        "<-02>+02",
        "<-2>+2",
        "<-05>+05",
        "<-5>+5",
        "<-07>+07",
        "<-7>+7"
      } ).ToArray();

      Connection.Close();
    }

    [Test]
    public void TimeZoneRecognitionTest()
    {
      foreach(var timezone in testTimezones) {

        if (timezone.StartsWith('<')) {
          if (timezone.Contains('0')) {

          
          Assert.That(serverTimeZones.TryGetValue(timezone, out var result1), Is.False);
          Assert.That(serverTimeZones.TryGetValue(PostgreSqlHelper.TryGetZoneFromPosix(timezone), out var result2), Is.True);
          Assert.That(result2, Is.EqualTo(TimeSpan.FromHours(2))
            .Or.EqualTo(TimeSpan.FromHours(5))
            .Or.EqualTo(TimeSpan.FromHours(7))
            .Or.EqualTo(TimeSpan.FromHours(-2))
            .Or.EqualTo(TimeSpan.FromHours(-5))
            .Or.EqualTo(TimeSpan.FromHours(-7)));
          }
          else {
            Assert.That(serverTimeZones.TryGetValue(timezone, out var result1), Is.False);
            Assert.That(serverTimeZones.TryGetValue(PostgreSqlHelper.TryGetZoneFromPosix(timezone), out var result2), Is.False);
          }

        }
        else {
          Assert.That(serverTimeZones.TryGetValue(timezone, out var result1), Is.True);
          Assert.That(serverTimeZones.TryGetValue(PostgreSqlHelper.TryGetZoneFromPosix(timezone), out var result2), Is.True);
          Assert.That(result1, Is.EqualTo(result2));
        }
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
  }
}
