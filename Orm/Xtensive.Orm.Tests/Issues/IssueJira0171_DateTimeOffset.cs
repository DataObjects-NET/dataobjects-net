// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.11.27

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0171_DateTimeOffsetModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0171_DateTimeOffsetModel
  {
    [HierarchyRoot]
    public class EntityWithDateTimeOffset : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public DateTimeOffset Today { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0171_DateTimeOffset : AutoBuildTest
  {
    private DateTimeOffset today = new DateTimeOffset(2018, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDateTimeOffset));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      var providerInfo = StorageProviderInfo.Instance.Info;
      if (providerInfo.ProviderName == WellKnown.Provider.PostgreSql) {
        var localZone = today.ToLocalTime().Offset;
        var localZoneString = ((localZone < TimeSpan.Zero) ? "-" : "+") + localZone.ToString(@"hh\:mm");
        configuration.ConnectionInitializationSql = string.Format("SET TIME ZONE INTERVAL '{0}' HOUR TO MINUTE", localZoneString);
      }
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e = new EntityWithDateTimeOffset {
          Today = today
        };
        tx.Complete();
      }
    }

    private void RunAllTestsInt(Func<int, Expression<Func<EntityWithDateTimeOffset, bool>>> filterProvider)
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        RunTest(session, filterProvider.Invoke(1));
        RunTest(session, filterProvider.Invoke(20));
        RunTest(session, filterProvider.Invoke(-5));
        RunTest(session, filterProvider.Invoke(0));
      }
    }

    private void RunAllTestsDouble(Func<double, Expression<Func<EntityWithDateTimeOffset, bool>>> filterProvider)
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        RunTest(session, filterProvider.Invoke(1));
        RunTest(session, filterProvider.Invoke(20));
        RunTest(session, filterProvider.Invoke(-5));
        RunTest(session, filterProvider.Invoke(0));
      }
    }

    private static void RunTest(Session session, Expression<Func<EntityWithDateTimeOffset, bool>> filter)
    {
      var count = session.Query.All<EntityWithDateTimeOffset>().Count(filter);
      Assert.That(count, Is.EqualTo(1));
    }

    private void RunAllTests(Expression<Func<EntityWithDateTimeOffset, bool>> filterProvider)
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        RunTest(session, filterProvider);
      }
    }

    # region Test Methods For Adding

    [Test]
    public void AddYearsTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsInt(value => e => e.Today.AddYears(value)==todayLocal.AddYears(value));
    }

    [Test]
    public void AddMonthsTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsInt(value => e => e.Today.AddMonths(value)==todayLocal.AddMonths(value));
    }

    [Test]
    public void AddDaysTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsDouble(value => e => e.Today.AddDays(value)==todayLocal.AddDays(value));
    }

    [Test]
    public void AddHoursTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsDouble(value => e => e.Today.AddHours(value)==todayLocal.AddHours(value));
    }

    [Test]
    public void AddMinutesTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsDouble(value => e => e.Today.AddMinutes(value)==todayLocal.AddMinutes(value));
    }

    [Test]
    public void AddSecondsTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsDouble(value => e => e.Today.AddSeconds(value)==todayLocal.AddSeconds(value));
    }

    [Test]
    public void AddMillisecondsTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTestsDouble(value => e => e.Today.AddMilliseconds(value)==todayLocal.AddMilliseconds(value));
    }

    [Test]
    public void AddTest()
    {
      TimeSpan timeSpan = new TimeSpan(-6, 00, 0);
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Add(timeSpan)==todayLocal.Add(timeSpan));
    }

    # endregion

    # region Tests Extractors

    [Test]
    public void YearTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Year==todayLocal.Year);
    }

    [Test]
    public void MonthTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Month==todayLocal.Month);
    }

    [Test]
    public void DayTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Day==todayLocal.Day);
    }

    [Test]
    public void HourTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Hour==todayLocal.Hour);
    }

    [Test]
    public void MinuteTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Minute==todayLocal.Minute);
    }

    [Test]
    public void SecondTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Second==todayLocal.Second);
    }

    [Test]
    public void MilliseconsTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Millisecond==todayLocal.Millisecond);
    }

    [Test]
    public void TimeOfDayTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.TimeOfDay==todayLocal.TimeOfDay);
    }

    [Test]
    public void DateTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Date==todayLocal.Date);
    }

    [Test]
    public void DayOfWeekTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.DayOfWeek==todayLocal.DayOfWeek);
    }

    [Test]
    public void DayOfYearTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.DayOfYear==todayLocal.DayOfYear);
    }

    [Test]
    public void DateTimeTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.DateTime==todayLocal.DateTime);
    }

    [Test]
    public void OffsetTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Offset==todayLocal.Offset);
    }

    [Test]
    public void UtcDateTimeTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.UtcDateTime==todayLocal.UtcDateTime);
    }

    [Test]
    public void LocalDateTimeTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.LocalDateTime==todayLocal.LocalDateTime);
    }

    # endregion

    #region Tests Operators

    [Test]
    public void EqualityTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today==todayLocal);
    }

    [Test]
    public void InequalityTets()
    {
      DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
      var nowLocal = TryMoveToLocalTimeZone(dateTimeOffset);
      RunAllTests(e => e.Today!=nowLocal);
    }

    [Test]
    public void GreaterThanTest()
    {
      DateTimeOffset dateTimeOffset = new DateTimeOffset(2012, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));
      var dtoLocal = TryMoveToLocalTimeZone(dateTimeOffset);
      RunAllTests(e => e.Today > dtoLocal);
    }

    [Test]
    public void GreaterThanOrEqual()
    {
      DateTimeOffset dateTimeOffset = new DateTimeOffset(2012, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));
      var todayLocal = TryMoveToLocalTimeZone(today);
      var dtoLocal = TryMoveToLocalTimeZone(dateTimeOffset);
      RunAllTests(e => e.Today >= dtoLocal);
      RunAllTests(e => e.Today >= todayLocal);
    }

    [Test]
    public void LessThan()
    {
      var nowLocal = TryMoveToLocalTimeZone(DateTimeOffset.Now);
      RunAllTests(e => e.Today < nowLocal);
    }

    [Test]
    public void LessThanOrEqual()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today <= DateTimeOffset.Now);
      RunAllTests(e => e.Today <= today);
    }

    [Test]
    public void AdditionTest()
    {
      TimeSpan timeSpan = new TimeSpan(1, 2, 3, 4, 5);
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => (e.Today + timeSpan)==(todayLocal + timeSpan));
    }

    [Test]
    public void SubtractDateTimeOffsetTest()
    {
      DateTimeOffset subtractDateTimeOffset = new DateTimeOffset(2013, 11, 27, 10, 0, 0, 0, new TimeSpan(-1, 0, 0));
      var todayLocal = TryMoveToLocalTimeZone(today);
      var substractDTOLocal = TryMoveToLocalTimeZone(subtractDateTimeOffset);
      RunAllTests(e => e.Today.Subtract(substractDTOLocal)==todayLocal.Subtract(substractDTOLocal));
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.Subtract(new TimeSpan(1, 10, 2, 1))==todayLocal.Subtract(new TimeSpan(1, 10, 2, 1)));
    }

    #endregion

    [Test]
    public void ToUniversalTimeTest()
    {
      var todayLocal = TryMoveToLocalTimeZone(today);
      RunAllTests(e => e.Today.ToUniversalTime()==todayLocal.ToUniversalTime());
    }

    [Test]
    public void ToLocalTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query =
          from t in session.Query.All<EntityWithDateTimeOffset>()
          group t by new {
            Date = t.Today.ToLocalTime(),
            ServerOffset = t.Today.ToLocalTime().Offset
          };

        var resultQuery = query.ToList().FirstOrDefault();

        if (resultQuery!=null) {
          var serverOffset = new TimeSpan(resultQuery.Key.ServerOffset.Hours, resultQuery.Key.ServerOffset.Minutes, 0);
          var todayLocal = TryMoveToLocalTimeZone(today);
          Assert.That(resultQuery.Key.Date, Is.EqualTo(todayLocal.ToOffset(serverOffset)));
        }
        tx.Complete();
      }
    }

    [Test]
    public void OutputDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<EntityWithDateTimeOffset>()
          .Select(d => d.Today)
          .ToList();
        var todayLocal = TryMoveToLocalTimeZone(today);
        Assert.That(result[0], Is.EqualTo(todayLocal));
        tx.Complete();
      }
    }

    protected DateTimeOffset TryMoveToLocalTimeZone(DateTimeOffset dateTimeOffset)
    {
      if (ProviderInfo.ProviderName==WellKnown.Provider.PostgreSql)
        return dateTimeOffset.ToLocalTime();
      return dateTimeOffset;
    }
  }
}
