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
    private DateTimeOffset today = new DateTimeOffset(2013, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDateTimeOffset));
      return configuration;
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
      RunAllTestsInt(value => e => e.Today.AddYears(value)==today.AddYears(value));
    }

    [Test]
    public void AddMonthsTest()
    {
      RunAllTestsInt(value => e => e.Today.AddMonths(value)==today.AddMonths(value));
    }

    [Test]
    public void AddDaysTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddDays(value)==today.AddDays(value));
    }

    [Test]
    public void AddHoursTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddHours(value)==today.AddHours(value));
    }

    [Test]
    public void AddMinutesTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddMinutes(value)==today.AddMinutes(value));
    }

    [Test]
    public void AddSecondsTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddSeconds(value)==today.AddSeconds(value));
    }

    [Test]
    public void AddMillisecondsTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddMilliseconds(value)==today.AddMilliseconds(value));
    }

    [Test]
    public void AddTest()
    {
      TimeSpan timeSpan = new TimeSpan(-6, 00, 0);
      RunAllTests(e => e.Today.Add(timeSpan)==today.Add(timeSpan));
    }

    # endregion

    # region Tests Extractors

    [Test]
    public void YearTest()
    {
      RunAllTests(e => e.Today.Year==today.Year);
    }

    [Test]
    public void MonthTest()
    {
      RunAllTests(e => e.Today.Month==today.Month);
    }

    [Test]
    public void DayTest()
    {
      RunAllTests(e => e.Today.Day==today.Day);
    }

    [Test]
    public void HourTest()
    {
      RunAllTests(e => e.Today.Hour==today.Hour);
    }

    [Test]
    public void MinuteTest()
    {
      RunAllTests(e => e.Today.Minute==today.Minute);
    }

    [Test]
    public void SecondTest()
    {
      RunAllTests(e => e.Today.Second==today.Second);
    }

    [Test]
    public void MilliseconsTest()
    {
      RunAllTests(e => e.Today.Millisecond==today.Millisecond);
    }

    [Test]
    public void TimeOfDayTest()
    {
      RunAllTests(e => e.Today.TimeOfDay==today.TimeOfDay);
    }

    [Test]
    public void DateTest()
    {
      RunAllTests(e => e.Today.Date==today.Date);
    }

    [Test]
    public void DayOfWeekTest()
    {
      RunAllTests(e => e.Today.DayOfWeek==today.DayOfWeek);
    }

    [Test]
    public void DayOfYearTest()
    {
      RunAllTests(e => e.Today.DayOfYear==today.DayOfYear);
    }

    [Test]
    public void DateTimeTest()
    {
      RunAllTests(e => e.Today.DateTime==today.DateTime);
    }

    [Test]
    public void OffsetTest()
    {
      RunAllTests(e => e.Today.Offset==today.Offset);
    }

    [Test]
    public void UtcDateTimeTest()
    {
      RunAllTests(e => e.Today.UtcDateTime==today.UtcDateTime);
    }

    [Test]
    public void LocalDateTimeTest()
    {
      RunAllTests(e => e.Today.LocalDateTime==today.LocalDateTime);
    }

    # endregion

    #region Tests Operators

    [Test]
    public void EqualityTest()
    {
      RunAllTests(e => e.Today==today);
    }

    [Test]
    public void InequalityTets()
    {
      DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
      RunAllTests(e => e.Today!=dateTimeOffset);
    }

    [Test]
    public void GreaterThanTest()
    {
      DateTimeOffset dateTimeOffset = new DateTimeOffset(2012, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));
      RunAllTests(e => e.Today > dateTimeOffset);
    }

    [Test]
    public void GreaterThanOrEqual()
    {
      DateTimeOffset dateTimeOffset = new DateTimeOffset(2012, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));
      RunAllTests(e => e.Today >= dateTimeOffset);
      RunAllTests(e => e.Today >= today);
    }

    [Test]
    public void LessThan()
    {
      RunAllTests(e => e.Today < DateTimeOffset.Now);
    }

    [Test]
    public void LessThanOrEqual()
    {
      RunAllTests(e => e.Today <= DateTimeOffset.Now);
      RunAllTests(e => e.Today <= today);
    }

    [Test]
    public void AdditionTest()
    {
      TimeSpan timeSpan = new TimeSpan(1, 2, 3, 4, 5);
      RunAllTests(e => (e.Today + timeSpan)==(today + timeSpan));
    }

    [Test]
    public void SubtractDateTimeOffsetTest()
    {
      DateTimeOffset subtractDateTimeOffset = new DateTimeOffset(2013, 11, 27, 10, 0, 0, 0, new TimeSpan(-1, 0, 0));
      RunAllTests(e => e.Today.Subtract(subtractDateTimeOffset)==today.Subtract(subtractDateTimeOffset));
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      RunAllTests(e => e.Today.Subtract(new TimeSpan(1, 10, 2, 1))==today.Subtract(new TimeSpan(1, 10, 2, 1)));
    }

    #endregion

    [Test]
    public void ToLocalTimeTest()
    {
      DateTimeOffset todayAssert = new DateTimeOffset(2013, 11, 28, 16, 43, 0, 0, new TimeSpan(4, 0, 0));

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q =
          from t in session.Query.All<EntityWithDateTimeOffset>()
          group t by new {
            Date = t.Today.ToLocalTime()
          };
        Assert.That(q.ToList()[0].Key.Date, Is.EqualTo(todayAssert));
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
        Assert.That(result[0], Is.EqualTo(today));
        tx.Complete();
      }
    }
  }
}
