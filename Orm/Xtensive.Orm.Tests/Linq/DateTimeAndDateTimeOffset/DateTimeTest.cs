// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.26

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeTestModels;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  namespace DateTimeTestModels
  {
    [HierarchyRoot]
    public class SingleEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTime DateTime { get; set; }
    }

    [HierarchyRoot]
    public class MultipleEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTime DateTime { get; set; }
    }
  }

  public class DateTimeTest : BaseDateTimeAndDateTimeOffsetTest
  {
    private class JoinResult
    {
      public int LeftId { get; set; }
      public int RightId { get; set; }
      public DateTime LeftDateTime { get; set; }
      public DateTime RightDateTime { get; set; }

      public override bool Equals(object obj)
      {
        var equalTo = obj as JoinResult;
        if (equalTo==null)
          return false;
        return LeftId==equalTo.LeftId && RightId==equalTo.RightId && LeftDateTime==equalTo.LeftDateTime && RightDateTime==equalTo.RightDateTime;
      }
    }

    #region Default values

    public virtual DateTime DefaultDateTime
    {
      get { return new DateTime(2016, 01, 02, 03, 04, 05, DateTimeKind.Unspecified); }
    }

    public virtual DateTime WrongDateTime
    {
      get { return new DateTime(2017, 12, 11, 10, 09, 08, DateTimeKind.Unspecified); }
    }

    public virtual DateTime DateTimeForSubstract
    {
      get { return new DateTime(2015, 12, 11, 10, 9, 8, DateTimeKind.Unspecified); }
    }

    public virtual TimeSpan DefaultTimeSpan
    {
      get { return new TimeSpan(5, 4, 3, 2); }
    }

    public virtual TimeSpan MultipleEntityDateStep
    {
      get { return TimeSpan.FromSeconds(1); }
    }

    public virtual int MultipleEntityCount
    {
      get { return 20; }
    }

    #endregion

    [Test]
    public void ExtractDateTimeTest()
    {
      OpenSessionAndAction(ExtractDateTimeProtected);
    }

    [Test]
    public void DateTimeOperationsTest()
    {
      OpenSessionAndAction(DateTimeOperationsProtected);
    }

    [Test]
    public void DateTimeCompareWithDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest(c => c.DateTime==DefaultDateTime);
        RunTest(c => c.DateTime.Date==DefaultDateTime.Date);
        RunTest(c => c.DateTime > DefaultDateTime.AddMinutes(-1));
        RunWrongTest(c => c.DateTime < DefaultDateTime.AddMinutes(-1));
      });
    }

    [Test]
    public void ToIsoStringTest()
    {
      OpenSessionAndAction(() => {
        RunTest(c => c.DateTime.ToString("s")==DefaultDateTime.ToString("s"));
        RunWrongTest(c => c.DateTime.ToString("s")==DefaultDateTime.AddMinutes(1).ToString("s"));
      });
    }

    //    [Test] Not supported
    public void ConvertToAnotherKindTest()
    {
      OpenSessionAndAction(ConvertToAnotherKindProtected);
    }

    #region Build config and populate data

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (SingleEntity));
      configuration.Types.Register(typeof (MultipleEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateDataProtected()
    {
      new SingleEntity { DateTime = DefaultDateTime };

      new MultipleEntity { DateTime = DefaultDateTime };
      new MultipleEntity { DateTime = DefaultDateTime };
      new MultipleEntity { DateTime = DefaultDateTime.Date };
      new MultipleEntity { DateTime = new DateTime(DefaultDateTime.Ticks, DateTimeKind.Local) };
      new MultipleEntity { DateTime = new DateTime(DefaultDateTime.Ticks, DateTimeKind.Utc) };
      new MultipleEntity { DateTime = DefaultDateTime.AddYears(1) };
      new MultipleEntity { DateTime = DefaultDateTime.AddYears(-2) };
      new MultipleEntity { DateTime = DefaultDateTime.AddMonths(44) };
      new MultipleEntity { DateTime = DefaultDateTime.AddMonths(-55) };
      new MultipleEntity { DateTime = DefaultDateTime.AddHours(5) };
      new MultipleEntity { DateTime = DefaultDateTime.AddHours(-5) };
      new MultipleEntity { DateTime = DefaultDateTime.AddMinutes(59) };
      new MultipleEntity { DateTime = DefaultDateTime.AddMinutes(-49) };
      new MultipleEntity { DateTime = DefaultDateTime.AddSeconds(1) };
      new MultipleEntity { DateTime = DefaultDateTime.AddMinutes(-2) };

      var dateTime = DefaultDateTime;
      for (var i = 0; i < MultipleEntityCount; ++i) {
        new MultipleEntity { DateTime = dateTime };
        dateTime = dateTime.Add(MultipleEntityDateStep);
      }
    }

    #endregion

    #region Helpers

    protected void RunTest(Expression<Func<SingleEntity, bool>> expression)
    {
      RunTest<SingleEntity>(expression);
    }

    protected void RunWrongTest(Expression<Func<SingleEntity, bool>> expression)
    {
      RunWrongTest<SingleEntity>(expression);
    }

    #endregion

    #region Implementation of own tests

    protected virtual void ExtractDateTimeProtected()
    {
      RunTest(c => c.DateTime==DefaultDateTime);
      RunTest(c => c.DateTime.Year==DefaultDateTime.Year);
      RunTest(c => c.DateTime.Month==DefaultDateTime.Month);
      RunTest(c => c.DateTime.Day==DefaultDateTime.Day);
      RunTest(c => c.DateTime.Hour==DefaultDateTime.Hour);
      RunTest(c => c.DateTime.Minute==DefaultDateTime.Minute);
      RunTest(c => c.DateTime.Second==DefaultDateTime.Second);

      RunTest(c => c.DateTime.Date==DefaultDateTime.Date);
      RunTest(c => c.DateTime.TimeOfDay==DefaultDateTime.TimeOfDay);
      RunTest(c => c.DateTime.DayOfYear==DefaultDateTime.DayOfYear);
      RunTest(c => c.DateTime.DayOfWeek==DefaultDateTime.DayOfWeek);

      RunWrongTest(c => c.DateTime==WrongDateTime);
      RunWrongTest(c => c.DateTime.Year==WrongDateTime.Year);
      RunWrongTest(c => c.DateTime.Month==WrongDateTime.Month);
      RunWrongTest(c => c.DateTime.Day==WrongDateTime.Day);
      RunWrongTest(c => c.DateTime.Hour==WrongDateTime.Hour);
      RunWrongTest(c => c.DateTime.Minute==WrongDateTime.Minute);
      RunWrongTest(c => c.DateTime.Second==WrongDateTime.Second);

      RunWrongTest(c => c.DateTime.Date==WrongDateTime.Date);
      RunWrongTest(c => c.DateTime.TimeOfDay==WrongDateTime.TimeOfDay);
      RunWrongTest(c => c.DateTime.DayOfYear==WrongDateTime.DayOfYear);
      RunWrongTest(c => c.DateTime.DayOfWeek==WrongDateTime.DayOfWeek);
    }

    protected virtual void DateTimeOperationsProtected()
    {
      RunTest(c => c.DateTime==DefaultDateTime);
      RunTest(c => c.DateTime.AddYears(1)==DefaultDateTime.AddYears(1));
      RunTest(c => c.DateTime.AddMonths(1)==DefaultDateTime.AddMonths(1));
      RunTest(c => c.DateTime.AddDays(1)==DefaultDateTime.AddDays(1));
      RunTest(c => c.DateTime.AddHours(1)==DefaultDateTime.AddHours(1));
      RunTest(c => c.DateTime.AddMinutes(1)==DefaultDateTime.AddMinutes(1));
      RunTest(c => c.DateTime.AddSeconds(1)==DefaultDateTime.AddSeconds(1));
      RunTest(c => c.DateTime.Add(DefaultTimeSpan)==DefaultDateTime.Add(DefaultTimeSpan));
      RunTest(c => c.DateTime.Subtract(DefaultTimeSpan)==DefaultDateTime.Subtract(DefaultTimeSpan));
      RunTest(c => c.DateTime.Subtract(DateTimeForSubstract)==DefaultDateTime.Subtract(DateTimeForSubstract));

      RunTest(c => c.DateTime + DefaultTimeSpan==DefaultDateTime + DefaultTimeSpan);
      RunTest(c => c.DateTime - DefaultTimeSpan==DefaultDateTime - DefaultTimeSpan);
      RunTest(c => c.DateTime - DateTimeForSubstract==DefaultDateTime - DateTimeForSubstract);

      RunWrongTest(c => c.DateTime==WrongDateTime);
      RunWrongTest(c => c.DateTime.AddYears(1)==WrongDateTime.AddYears(1));
      RunWrongTest(c => c.DateTime.AddMonths(1)==WrongDateTime.AddMonths(1));
      RunWrongTest(c => c.DateTime.AddDays(1)==WrongDateTime.AddDays(1));
      RunWrongTest(c => c.DateTime.AddHours(1)==WrongDateTime.AddHours(1));
      RunWrongTest(c => c.DateTime.AddMinutes(1)==WrongDateTime.AddMinutes(1));
      RunWrongTest(c => c.DateTime.AddSeconds(1)==WrongDateTime.AddSeconds(1));
      RunWrongTest(c => c.DateTime.Add(DefaultTimeSpan)==WrongDateTime.Add(DefaultTimeSpan));
      RunWrongTest(c => c.DateTime.Subtract(DefaultTimeSpan)==WrongDateTime.Subtract(DefaultTimeSpan));
      RunWrongTest(c => c.DateTime.Subtract(DateTimeForSubstract)==WrongDateTime.Subtract(DateTimeForSubstract));

      RunWrongTest(c => c.DateTime + DefaultTimeSpan==WrongDateTime + DefaultTimeSpan);
      RunWrongTest(c => c.DateTime - DefaultTimeSpan==WrongDateTime - DefaultTimeSpan);
      RunWrongTest(c => c.DateTime - DateTimeForSubstract==WrongDateTime - DateTimeForSubstract);
    }

    protected virtual void ConvertToAnotherKindProtected()
    {
      RunTest(c => c.DateTime.ToUniversalTime()==DefaultDateTime.ToUniversalTime());
      RunTest(c => c.DateTime.ToLocalTime()==DefaultDateTime.ToLocalTime());

      RunWrongTest(c => c.DateTime.ToUniversalTime()==WrongDateTime.ToUniversalTime());
      RunWrongTest(c => c.DateTime.ToLocalTime()==WrongDateTime.ToLocalTime());
    }

    #endregion

    #region Implementation of ancestor's tests

    protected override void OrderByProtected()
    {
      OrderByProtected<MultipleEntity, DateTime, int>(c => c.DateTime, c => c.Id);
      OrderByProtected<MultipleEntity, DateTime, DateTime>(c => c.DateTime, c => c);
    }

    protected override void DistinctProtected()
    {
      DistinctProtected<MultipleEntity, DateTime>(c => c.DateTime);
    }

    protected override void MinMaxProtected()
    {
      MinMaxProtected<MultipleEntity, DateTime>(c => c.DateTime);
    }

    protected override void GroupByProtected()
    {
      GroupByProtected<MultipleEntity, DateTime, int>(c => c.DateTime, c => c.Id);
    }

    protected override void JoinProtected()
    {
      JoinProtected<MultipleEntity, MultipleEntity, JoinResult, DateTime, int>(
        left => left.DateTime,
        right => right.DateTime,
        (left, right) => new JoinResult { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTime, RightDateTime = right.DateTime },
        c => c.LeftId,
        c => c.RightId
        );
    }

    protected override void EqualsProtected()
    {
      RunTest(c => c.DateTime==DefaultDateTime);
      RunWrongTest(c => c.DateTime==WrongDateTime);
    }

    protected override void SkipTakeProtected()
    {
      SkipTakeProctected<MultipleEntity, DateTimeOffset, int>(c => c.DateTime, c => c.Id, 3, 5);
    }

    #endregion
  }
}
