// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.04.26

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.DateTimeOffsetTestModel;

namespace Xtensive.Orm.Tests.Linq.DateTimeOffsetTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public DateTimeOffset DateWithOffset { get; set; }

    [Field]
    public DateTimeOffset? NullableDateDateWithOffset { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class DateTimeAndDateTimeOffsetTest : AutoBuildTest
  {
    private const string FirstEntityName = "FirstEntity";

    private static readonly TimeSpan BigTimeSpan = new TimeSpan(5, 4, 3, 2, 333);
    private static readonly TimeSpan DefaultOffset1 = TimeSpan.FromHours(-2.75);
    private static readonly TimeSpan DefaultOffset2 = TimeSpan.FromHours(3);
    private static readonly TimeSpan DefaultOffset3 = TimeSpan.FromHours(-3.25);
    private static readonly DateTime DefaultDateTime = new DateTime(2016, 04, 27, 13, 14, 15, 333);
    private static readonly DateTimeOffset DefaultDateTimeOffset = new DateTimeOffset(DefaultDateTime, DefaultOffset1);
    private static readonly DateTime WrongDateTime = DefaultDateTime.AddYears(1).AddMonths(1).Add(new TimeSpan(1, 1, 1, 1, 333));
    private static readonly DateTime WrongDateTime2 = WrongDateTime.AddYears(1).AddMonths(1).Add(new TimeSpan(1, 1, 1, 1, 333));
    private static readonly DateTimeOffset WrongDateTimeOffset = new DateTimeOffset(WrongDateTime, DefaultOffset3);

    [Test]
    public void EqualsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = Query.All<TestEntity>().First(c => c.Name==FirstEntityName);
        Assert.AreEqual(entity.Date, DefaultDateTime);
        Assert.AreEqual(entity.DateWithOffset, DefaultDateTimeOffset);
        Assert.AreEqual(entity.DateWithOffset, DefaultDateTimeOffset.ToOffset(DefaultOffset2));
        Assert.AreEqual(entity.DateWithOffset.ToOffset(DefaultOffset2), DefaultDateTimeOffset);

        Assert.AreNotEqual(entity.Date, WrongDateTime);
        Assert.AreNotEqual(entity.DateWithOffset, WrongDateTimeOffset);
        Assert.AreNotEqual(entity.DateWithOffset, WrongDateTimeOffset.ToOffset(DefaultOffset2));
        Assert.AreNotEqual(entity.DateWithOffset.ToOffset(DefaultOffset2), WrongDateTimeOffset);
      }
    }

    [Test]
    public void DifferentCulturesTest()
    {
      var oldCulture = Thread.CurrentThread.CurrentCulture;
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        EqualsTest();
      }
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
        EqualsTest();
      }

      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        EqualsTest();
      }
    }

    [Test]
    public void ExtractDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.Date==DefaultDateTime);
        RunTest(c => c.Date.Year==DefaultDateTime.Year);
        RunTest(c => c.Date.Month==DefaultDateTime.Month);
        RunTest(c => c.Date.Day==DefaultDateTime.Day);
        RunTest(c => c.Date.Hour==DefaultDateTime.Hour);
        RunTest(c => c.Date.Minute==DefaultDateTime.Minute);
        RunTest(c => c.Date.Second==DefaultDateTime.Second);

        RunTest(c => c.Date.Date==DefaultDateTime.Date);
        RunTest(c => c.Date.TimeOfDay==DefaultDateTime.TimeOfDay);
        RunTest(c => c.Date.DayOfYear==DefaultDateTime.DayOfYear);
        RunTest(c => c.Date.DayOfWeek==DefaultDateTime.DayOfWeek);

        RunWrongTest(c => c.Date==WrongDateTime);
        RunWrongTest(c => c.Date.Year==WrongDateTime.Year);
        RunWrongTest(c => c.Date.Month==WrongDateTime.Month);
        RunWrongTest(c => c.Date.Day==WrongDateTime.Day);
        RunWrongTest(c => c.Date.Hour==WrongDateTime.Hour);
        RunWrongTest(c => c.Date.Minute==WrongDateTime.Minute);
        RunWrongTest(c => c.Date.Second==WrongDateTime.Second);

        RunWrongTest(c => c.Date.Date==WrongDateTime.Date);
        RunWrongTest(c => c.Date.TimeOfDay==WrongDateTime.TimeOfDay);
        RunWrongTest(c => c.Date.DayOfYear==WrongDateTime.DayOfYear);
        RunWrongTest(c => c.Date.DayOfWeek==WrongDateTime.DayOfWeek);
      }
    }

    [Test]
    public void ExtractDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset);
        RunTest(c => c.DateWithOffset.Year==DefaultDateTimeOffset.Year);
        RunTest(c => c.DateWithOffset.Month==DefaultDateTimeOffset.Month);
        RunTest(c => c.DateWithOffset.Day==DefaultDateTimeOffset.Day);
        RunTest(c => c.DateWithOffset.Hour==DefaultDateTimeOffset.Hour);
        RunTest(c => c.DateWithOffset.Minute==DefaultDateTimeOffset.Minute);
        RunTest(c => c.DateWithOffset.Second==DefaultDateTimeOffset.Second);

        RunTest(c => c.DateWithOffset.Offset==DefaultDateTimeOffset.Offset);
        RunTest(c => c.DateWithOffset.Offset.Hours==DefaultDateTimeOffset.Offset.Hours);
        RunTest(c => c.DateWithOffset.Offset.Minutes==DefaultDateTimeOffset.Offset.Minutes);

        RunTest(c => c.DateWithOffset.TimeOfDay==DefaultDateTimeOffset.TimeOfDay);
        RunTest(c => c.DateWithOffset.Date==DefaultDateTimeOffset.Date);
        RunTest(c => c.DateWithOffset.DateTime==DefaultDateTimeOffset.DateTime);
        RunTest(c => c.DateWithOffset.DayOfWeek==DefaultDateTimeOffset.DayOfWeek);
        RunTest(c => c.DateWithOffset.DayOfYear==DefaultDateTimeOffset.DayOfYear);
        RunTest(c => c.DateWithOffset.LocalDateTime==DefaultDateTimeOffset.LocalDateTime);
        //        RunTest(c => c.DateWithOffset.UtcDateTime==DefaultDateTimeOffset.UtcDateTime);

        RunWrongTest(c => c.DateWithOffset==WrongDateTimeOffset);
        RunWrongTest(c => c.DateWithOffset.Year==WrongDateTimeOffset.Year);
        RunWrongTest(c => c.DateWithOffset.Month==WrongDateTimeOffset.Month);
        RunWrongTest(c => c.DateWithOffset.Day==WrongDateTimeOffset.Day);
        RunWrongTest(c => c.DateWithOffset.Hour==WrongDateTimeOffset.Hour);
        RunWrongTest(c => c.DateWithOffset.Minute==WrongDateTimeOffset.Minute);
        RunWrongTest(c => c.DateWithOffset.Second==WrongDateTimeOffset.Second);

        RunWrongTest(c => c.DateWithOffset.Offset==WrongDateTimeOffset.Offset);
        RunWrongTest(c => c.DateWithOffset.Offset.Hours==WrongDateTimeOffset.Offset.Hours);
        RunWrongTest(c => c.DateWithOffset.Offset.Minutes==WrongDateTimeOffset.Offset.Minutes);

        RunWrongTest(c => c.DateWithOffset.TimeOfDay==WrongDateTimeOffset.TimeOfDay);
        RunWrongTest(c => c.DateWithOffset.Date==WrongDateTimeOffset.Date);
        RunWrongTest(c => c.DateWithOffset.DateTime==WrongDateTimeOffset.DateTime);
        RunWrongTest(c => c.DateWithOffset.DayOfWeek==WrongDateTimeOffset.DayOfWeek);
        RunWrongTest(c => c.DateWithOffset.DayOfYear==WrongDateTimeOffset.DayOfYear);
        RunWrongTest(c => c.DateWithOffset.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
        //        RunTest(c => c.DateWithOffset.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
      }
    }

    [Test]
    public void DateTimeOperationsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.Date==DefaultDateTime);
        RunTest(c => c.Date.AddYears(1)==DefaultDateTime.AddYears(1));
        RunTest(c => c.Date.AddMonths(1)==DefaultDateTime.AddMonths(1));
        RunTest(c => c.Date.AddDays(1)==DefaultDateTime.AddDays(1));
        RunTest(c => c.Date.AddHours(1)==DefaultDateTime.AddHours(1));
        RunTest(c => c.Date.AddMinutes(1)==DefaultDateTime.AddMinutes(1));
        RunTest(c => c.Date.AddSeconds(1)==DefaultDateTime.AddSeconds(1));
        RunTest(c => c.Date.Add(BigTimeSpan)==DefaultDateTime.Add(BigTimeSpan));
        RunTest(c => c.Date.Subtract(DefaultOffset3)==DefaultDateTime.Subtract(DefaultOffset3));
        RunTest(c => c.Date.Subtract(WrongDateTime)==DefaultDateTime.Subtract(WrongDateTime));
        //        RunTest(c => c.Date.ToLocalTime() == DefaultDateTime.ToLocalTime());

        RunTest(c => c.Date + BigTimeSpan==DefaultDateTime + BigTimeSpan);
        RunTest(c => c.Date - BigTimeSpan==DefaultDateTime - BigTimeSpan);
        RunTest(c => c.Date - WrongDateTime==DefaultDateTime - WrongDateTime);

        RunWrongTest(c => c.Date==WrongDateTime);
        RunWrongTest(c => c.Date.AddYears(1)==WrongDateTime.AddYears(1));
        RunWrongTest(c => c.Date.AddMonths(1)==WrongDateTime.AddMonths(1));
        RunWrongTest(c => c.Date.AddDays(1)==WrongDateTime.AddDays(1));
        RunWrongTest(c => c.Date.AddHours(1)==WrongDateTime.AddHours(1));
        RunWrongTest(c => c.Date.AddMinutes(1)==WrongDateTime.AddMinutes(1));
        RunWrongTest(c => c.Date.AddSeconds(1)==WrongDateTime.AddSeconds(1));
        RunWrongTest(c => c.Date.Add(BigTimeSpan)==WrongDateTime.Add(BigTimeSpan));
        RunWrongTest(c => c.Date.Subtract(DefaultOffset3)==WrongDateTime.Subtract(DefaultOffset3));
        RunWrongTest(c => c.Date.Subtract(WrongDateTime2)==WrongDateTime.Subtract(WrongDateTime2));
        //        RunWrongTest(c => c.Date.ToLocalTime() == WrongDateTime.ToLocalTime());

        RunWrongTest(c => c.Date + BigTimeSpan==WrongDateTime + BigTimeSpan);
        RunWrongTest(c => c.Date - BigTimeSpan==WrongDateTime - BigTimeSpan);
        RunWrongTest(c => c.Date - WrongDateTime2==WrongDateTime - WrongDateTime2);
      }
    }

    [Test]
    public void DateTimeOffsetOperationsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset);
        RunTest(c => c.DateWithOffset.AddYears(1)==DefaultDateTimeOffset.AddYears(1));
        RunTest(c => c.DateWithOffset.AddMonths(1)==DefaultDateTimeOffset.AddMonths(1));
        RunTest(c => c.DateWithOffset.AddDays(1)==DefaultDateTimeOffset.AddDays(1));
        RunTest(c => c.DateWithOffset.AddHours(1)==DefaultDateTimeOffset.AddHours(1));
        RunTest(c => c.DateWithOffset.AddMinutes(1)==DefaultDateTimeOffset.AddMinutes(1));
        RunTest(c => c.DateWithOffset.AddSeconds(1)==DefaultDateTimeOffset.AddSeconds(1));
        RunTest(c => c.DateWithOffset.Add(BigTimeSpan)==DefaultDateTimeOffset.Add(BigTimeSpan));
        RunTest(c => c.DateWithOffset.Subtract(DefaultOffset3)==DefaultDateTimeOffset.Subtract(DefaultOffset3));
        RunTest(c => c.DateWithOffset.Subtract(WrongDateTimeOffset)==DefaultDateTimeOffset.Subtract(WrongDateTimeOffset));
        //        RunTest(c => c.DateWithOffset.ToLocalTime() == DefaultDateTimeOffset.ToLocalTime());

        RunTest(c => c.DateWithOffset + BigTimeSpan==DefaultDateTimeOffset + BigTimeSpan);
        RunTest(c => c.DateWithOffset - BigTimeSpan==DefaultDateTimeOffset - BigTimeSpan);
        RunTest(c => c.DateWithOffset - WrongDateTimeOffset==DefaultDateTimeOffset - WrongDateTimeOffset);
        RunTest(c => c.DateWithOffset - WrongDateTime==DefaultDateTimeOffset - WrongDateTime);

        RunWrongTest(c => c.DateWithOffset==WrongDateTimeOffset);
        RunWrongTest(c => c.DateWithOffset.AddYears(1)==WrongDateTimeOffset.AddYears(1));
        RunWrongTest(c => c.DateWithOffset.AddMonths(1)==WrongDateTimeOffset.AddMonths(1));
        RunWrongTest(c => c.DateWithOffset.AddDays(1)==WrongDateTimeOffset.AddDays(1));
        RunWrongTest(c => c.DateWithOffset.AddHours(1)==WrongDateTimeOffset.AddHours(1));
        RunWrongTest(c => c.DateWithOffset.AddMinutes(1)==WrongDateTimeOffset.AddMinutes(1));
        RunWrongTest(c => c.DateWithOffset.AddSeconds(1)==WrongDateTimeOffset.AddSeconds(1));
        RunWrongTest(c => c.DateWithOffset.Add(BigTimeSpan)==WrongDateTimeOffset.Add(BigTimeSpan));
        RunWrongTest(c => c.DateWithOffset.Subtract(DefaultOffset3)==WrongDateTimeOffset.Subtract(DefaultOffset3));
        RunWrongTest(c => c.DateWithOffset.Subtract(WrongDateTimeOffset)==WrongDateTimeOffset.Subtract(WrongDateTimeOffset));
        //        RunWrongTest(c => c.DateWithOffset.ToLocalTime() == WrongDateTimeOffset.ToLocalTime());

        RunWrongTest(c => c.DateWithOffset + BigTimeSpan==WrongDateTimeOffset + BigTimeSpan);
        RunWrongTest(c => c.DateWithOffset - BigTimeSpan==WrongDateTimeOffset - BigTimeSpan);
        RunWrongTest(c => c.DateWithOffset - WrongDateTimeOffset==WrongDateTimeOffset - WrongDateTimeOffset);
        RunWrongTest(c => c.DateWithOffset - WrongDateTime==WrongDateTimeOffset - WrongDateTime);
      }
    }

    [Test]
    public void DateTimeCompareWithDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.Date==DefaultDateTime);
        RunTest(c => c.Date > DefaultDateTime.AddMinutes(-1));
        RunWrongTest(c => c.Date < DefaultDateTime.AddMinutes(-1));
      }
    }

    //    [Test] // Comparing datetime with datetimeoffset is not supported
    public void DateTimeCompareWithDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var defaultDateWithOffset = new DateTimeOffset(DefaultDateTime).ToOffset(DefaultOffset1);
        RunTest(c => c.Date==defaultDateWithOffset);
        RunTest(c => c.Date==defaultDateWithOffset.ToOffset(DefaultOffset1));
        RunTest(c => c.Date==defaultDateWithOffset.ToOffset(DefaultOffset2));
        RunTest(c => c.Date==defaultDateWithOffset.ToOffset(DefaultOffset3));
        RunTest(c => c.Date > defaultDateWithOffset.AddMinutes(-1));
        RunTest(c => c.Date > defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset1));
        RunTest(c => c.Date > defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset2));
        RunTest(c => c.Date > defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset3));
        RunWrongTest(c => c.Date < defaultDateWithOffset.AddMinutes(-1));
        RunWrongTest(c => c.Date < defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset1));
        RunWrongTest(c => c.Date < defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset2));
        RunWrongTest(c => c.Date < defaultDateWithOffset.AddMinutes(-1).ToOffset(DefaultOffset3));
      }
    }

    [Test]
    public void DateTimeOffsetCompareWithDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.UtcDateTime);
        RunTest(c => c.DateWithOffset > DefaultDateTimeOffset.UtcDateTime.AddMinutes(-1));
        RunWrongTest(c => c.DateWithOffset < DefaultDateTimeOffset.UtcDateTime.AddMinutes(-1));
      }
    }

    [Test]
    public void DateTimeOffsetCompareWithDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset);
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset1));
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset2));
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset3));

        RunTest(c => c.DateWithOffset > DefaultDateTimeOffset.AddMinutes(-1));
        RunTest(c => c.DateWithOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset1));
        RunTest(c => c.DateWithOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset2));
        RunTest(c => c.DateWithOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset3));

        RunWrongTest(c => c.DateWithOffset < DefaultDateTimeOffset.AddMinutes(-1));
        RunWrongTest(c => c.DateWithOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset1));
        RunWrongTest(c => c.DateWithOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset2));
        RunWrongTest(c => c.DateWithOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset3));
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new TestEntity {
          Name = FirstEntityName,
          Date = DefaultDateTime,
          DateWithOffset = DefaultDateTimeOffset
        };
        tx.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private void RunTest(Expression<Func<TestEntity, bool>> filter, int rightCount = 1)
    {
      var count = Query.All<TestEntity>().Count(filter);
      Assert.AreEqual(count, rightCount);
    }

    private void RunWrongTest(Expression<Func<TestEntity, bool>> filter)
    {
      RunTest(filter, 0);
    }
  }
}
