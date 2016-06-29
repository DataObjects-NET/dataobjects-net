// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.30

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsetTestModels;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  namespace DateTimeOffsetTestModels
  {
    [HierarchyRoot]
    public class SingleEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTimeOffset DateTimeOffset { get; set; }
    }

    [HierarchyRoot]
    public class MultipleEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTimeOffset DateTimeOffset { get; set; }
    }
  }

  public class DateTimeOffsetTest : BaseDateTimeAndDateTimeOffsetTest
  {
    private class JoinResult
    {
      public int LeftId { get; set; }
      public int RightId { get; set; }
      public DateTimeOffset LeftDateTimeOffset { get; set; }
      public DateTimeOffset RightDateTimeOffset { get; set; }

      public override bool Equals(object obj)
      {
        var equalTo = obj as JoinResult;
        if (equalTo==null)
          return false;
        return LeftId==equalTo.LeftId && RightId==equalTo.RightId && LeftDateTimeOffset==equalTo.LeftDateTimeOffset && RightDateTimeOffset==equalTo.RightDateTimeOffset;
      }
    }

    #region Default values

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

    public virtual TimeSpan DefaultOffset
    {
      get { return new TimeSpan(-2, 45, 0); }
    }

    public virtual TimeSpan SecondOffset
    {
      get { return new TimeSpan(11, 0, 0); }
    }

    public virtual TimeSpan ThirdOffset
    {
      get { return TimeSpan.Zero; }
    }

    public virtual DateTimeOffset DefaultDateTimeOffset
    {
      get { return new DateTimeOffset(2016, 01, 02, 03, 04, 05, DefaultOffset); }
    }

    public virtual DateTimeOffset WrongDateTimeOffset
    {
      get { return new DateTimeOffset(2017, 12, 11, 10, 9, 8, SecondOffset); }
    }

    public virtual DateTime DateTimeForSubstract
    {
      get { return new DateTime(2015, 11, 10, 9, 8, 7); }
    }

    public virtual DateTimeOffset DateTimeOffsetForSubstract
    {
      get { return new DateTimeOffset(2015, 11, 10, 9, 8, 7, SecondOffset); }
    }

    #endregion

    [Test]
    public void ExtractDateTimeOffsetTest()
    {
      OpenSessionAndAction(ExtractDateTimeOffsetProtected);
    }

    [Test]
    public void DateTimeOffsetOperationsTest()
    {
      OpenSessionAndAction(DateTimeOffsetOperationsProtected);
    }

    [Test]
    public void DateTimeOffsetCompareWithDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.UtcDateTime);
        RunTest(c => c.DateTimeOffset > DefaultDateTimeOffset.UtcDateTime.AddMinutes(-1));
        RunWrongTest(c => c.DateTimeOffset < DefaultDateTimeOffset.UtcDateTime.AddMinutes(-1));
      }
    }

    [Test]
    public void DateTimeOffsetCompareWithDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset);
        RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset));
        RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.ToOffset(SecondOffset));
        RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.ToOffset(ThirdOffset));

        RunTest(c => c.DateTimeOffset > DefaultDateTimeOffset.AddMinutes(-1));
        RunTest(c => c.DateTimeOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset));
        RunTest(c => c.DateTimeOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(SecondOffset));
        RunTest(c => c.DateTimeOffset > DefaultDateTimeOffset.AddMinutes(-1).ToOffset(ThirdOffset));

        RunWrongTest(c => c.DateTimeOffset < DefaultDateTimeOffset.AddMinutes(-1));
        RunWrongTest(c => c.DateTimeOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(DefaultOffset));
        RunWrongTest(c => c.DateTimeOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(SecondOffset));
        RunWrongTest(c => c.DateTimeOffset < DefaultDateTimeOffset.AddMinutes(-1).ToOffset(ThirdOffset));
      }
    }

    #region Build config, populate data and check requirements

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
      new SingleEntity { DateTimeOffset = DefaultDateTimeOffset };

      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.ToOffset(DefaultOffset) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.ToOffset(SecondOffset) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.ToOffset(ThirdOffset) };
      new MultipleEntity { DateTimeOffset = new DateTimeOffset(DefaultDateTimeOffset.Date, DefaultOffset) };
      new MultipleEntity { DateTimeOffset = new DateTimeOffset(DefaultDateTimeOffset.Date, SecondOffset) };
      new MultipleEntity { DateTimeOffset = new DateTimeOffset(DefaultDateTimeOffset.Date, ThirdOffset) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddYears(1) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddYears(1).ToOffset(SecondOffset) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddYears(-1) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddMonths(44) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddMonths(-55) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddHours(5) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddHours(-5) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddMinutes(59) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddMinutes(-49) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddSeconds(1) };
      new MultipleEntity { DateTimeOffset = DefaultDateTimeOffset.AddSeconds(-2) };

      var dateTime = DefaultDateTimeOffset;
      for (var i = 0; i < MultipleEntityCount; ++i) {
        new MultipleEntity { DateTimeOffset = dateTime };
        dateTime = dateTime.Add(MultipleEntityDateStep);
      }
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
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

    #region Implementation of ancestor's tests

    protected override void OrderByProtected()
    {
      OrderByProtected<MultipleEntity, DateTimeOffset, int>(c => c.DateTimeOffset, c => c.Id);
      OrderByProtected<MultipleEntity, DateTimeOffset, DateTimeOffset>(c => c.DateTimeOffset, c => c);
    }

    protected override void DistinctProtected()
    {
      DistinctProtected<MultipleEntity, DateTimeOffset>(c => c.DateTimeOffset);
    }

    protected override void MinMaxProtected()
    {
      MinMaxProtected<MultipleEntity, DateTimeOffset>(c => c.DateTimeOffset);
    }

    protected override void GroupByProtected()
    {
      GroupByProtected<MultipleEntity, DateTimeOffset, int>(c => c.DateTimeOffset, c => c.Id);
    }

    protected override void JoinProtected()
    {
      JoinProtected<MultipleEntity, MultipleEntity, JoinResult, DateTimeOffset, int>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult { LeftId = left.Id, RightId = right.Id, LeftDateTimeOffset = left.DateTimeOffset, RightDateTimeOffset = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId);
    }

    protected override void EqualsProtected()
    {
      RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset);
      RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.ToOffset(SecondOffset));
      RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset.ToOffset(ThirdOffset));

      RunWrongTest(c => c.DateTimeOffset==WrongDateTimeOffset);
      RunWrongTest(c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(SecondOffset));
      RunWrongTest(c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(ThirdOffset));
    }

    protected override void SkipTakeProtected()
    {
      SkipTakeProctected<MultipleEntity, DateTimeOffset, int>(c => c.DateTimeOffset, c => c.Id, 3, 5);
    }

    #endregion

    #region Implementation of own tests

    protected virtual void ExtractDateTimeOffsetProtected()
    {
      RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset);
      RunTest(c => c.DateTimeOffset.Year==DefaultDateTimeOffset.Year);
      RunTest(c => c.DateTimeOffset.Month==DefaultDateTimeOffset.Month);
      RunTest(c => c.DateTimeOffset.Day==DefaultDateTimeOffset.Day);
      RunTest(c => c.DateTimeOffset.Hour==DefaultDateTimeOffset.Hour);
      RunTest(c => c.DateTimeOffset.Minute==DefaultDateTimeOffset.Minute);
      RunTest(c => c.DateTimeOffset.Second==DefaultDateTimeOffset.Second);

      RunTest(c => c.DateTimeOffset.Offset==DefaultDateTimeOffset.Offset);
      RunTest(c => c.DateTimeOffset.Offset.Hours==DefaultDateTimeOffset.Offset.Hours);
      RunTest(c => c.DateTimeOffset.Offset.Minutes==DefaultDateTimeOffset.Offset.Minutes);

      RunTest(c => c.DateTimeOffset.TimeOfDay==DefaultDateTimeOffset.TimeOfDay);
      RunTest(c => c.DateTimeOffset.Date==DefaultDateTimeOffset.Date);
      RunTest(c => c.DateTimeOffset.DateTime==DefaultDateTimeOffset.DateTime);
      RunTest(c => c.DateTimeOffset.DayOfWeek==DefaultDateTimeOffset.DayOfWeek);
      RunTest(c => c.DateTimeOffset.DayOfYear==DefaultDateTimeOffset.DayOfYear);
      RunTest(c => c.DateTimeOffset.LocalDateTime==DefaultDateTimeOffset.LocalDateTime);
      //        RunTest(c => c.DateTimeOffset.UtcDateTime==DefaultDateTimeOffset.UtcDateTime);

      RunWrongTest(c => c.DateTimeOffset==WrongDateTimeOffset);
      RunWrongTest(c => c.DateTimeOffset.Year==WrongDateTimeOffset.Year);
      RunWrongTest(c => c.DateTimeOffset.Month==WrongDateTimeOffset.Month);
      RunWrongTest(c => c.DateTimeOffset.Day==WrongDateTimeOffset.Day);
      RunWrongTest(c => c.DateTimeOffset.Hour==WrongDateTimeOffset.Hour);
      RunWrongTest(c => c.DateTimeOffset.Minute==WrongDateTimeOffset.Minute);
      RunWrongTest(c => c.DateTimeOffset.Second==WrongDateTimeOffset.Second);

      RunWrongTest(c => c.DateTimeOffset.Offset==WrongDateTimeOffset.Offset);
      RunWrongTest(c => c.DateTimeOffset.Offset.Hours==WrongDateTimeOffset.Offset.Hours);
      RunWrongTest(c => c.DateTimeOffset.Offset.Minutes==WrongDateTimeOffset.Offset.Minutes);

      RunWrongTest(c => c.DateTimeOffset.TimeOfDay==WrongDateTimeOffset.TimeOfDay);
      RunWrongTest(c => c.DateTimeOffset.Date==WrongDateTimeOffset.Date);
      RunWrongTest(c => c.DateTimeOffset.DateTime==WrongDateTimeOffset.DateTime);
      RunWrongTest(c => c.DateTimeOffset.DayOfWeek==WrongDateTimeOffset.DayOfWeek);
      RunWrongTest(c => c.DateTimeOffset.DayOfYear==WrongDateTimeOffset.DayOfYear);
      RunWrongTest(c => c.DateTimeOffset.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
      //        RunTest(c => c.DateTimeOffset.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
    }

    protected virtual void DateTimeOffsetOperationsProtected()
    {
      RunTest(c => c.DateTimeOffset==DefaultDateTimeOffset);
      RunTest(c => c.DateTimeOffset.AddYears(1)==DefaultDateTimeOffset.AddYears(1));
      RunTest(c => c.DateTimeOffset.AddMonths(1)==DefaultDateTimeOffset.AddMonths(1));
      RunTest(c => c.DateTimeOffset.AddDays(1)==DefaultDateTimeOffset.AddDays(1));
      RunTest(c => c.DateTimeOffset.AddHours(1)==DefaultDateTimeOffset.AddHours(1));
      RunTest(c => c.DateTimeOffset.AddMinutes(1)==DefaultDateTimeOffset.AddMinutes(1));
      RunTest(c => c.DateTimeOffset.AddSeconds(1)==DefaultDateTimeOffset.AddSeconds(1));
      RunTest(c => c.DateTimeOffset.Add(DefaultTimeSpan)==DefaultDateTimeOffset.Add(DefaultTimeSpan));
      RunTest(c => c.DateTimeOffset.Subtract(DateTimeForSubstract)==DefaultDateTimeOffset.Subtract(DateTimeForSubstract));
      RunTest(c => c.DateTimeOffset.Subtract(DateTimeOffsetForSubstract)==DefaultDateTimeOffset.Subtract(DateTimeOffsetForSubstract));
      //        RunTest(c => c.DateTimeOffset.ToLocalTime() == DefaultDateTimeOffset.ToLocalTime());

      RunTest(c => c.DateTimeOffset + DefaultTimeSpan==DefaultDateTimeOffset + DefaultTimeSpan);
      RunTest(c => c.DateTimeOffset - DefaultTimeSpan==DefaultDateTimeOffset - DefaultTimeSpan);
      RunTest(c => c.DateTimeOffset - WrongDateTimeOffset==DefaultDateTimeOffset - WrongDateTimeOffset);
      RunTest(c => c.DateTimeOffset - DateTimeOffsetForSubstract==DefaultDateTimeOffset - DateTimeOffsetForSubstract);

      RunWrongTest(c => c.DateTimeOffset==WrongDateTimeOffset);
      RunWrongTest(c => c.DateTimeOffset.AddYears(1)==WrongDateTimeOffset.AddYears(1));
      RunWrongTest(c => c.DateTimeOffset.AddMonths(1)==WrongDateTimeOffset.AddMonths(1));
      RunWrongTest(c => c.DateTimeOffset.AddDays(1)==WrongDateTimeOffset.AddDays(1));
      RunWrongTest(c => c.DateTimeOffset.AddHours(1)==WrongDateTimeOffset.AddHours(1));
      RunWrongTest(c => c.DateTimeOffset.AddMinutes(1)==WrongDateTimeOffset.AddMinutes(1));
      RunWrongTest(c => c.DateTimeOffset.AddSeconds(1)==WrongDateTimeOffset.AddSeconds(1));
      RunWrongTest(c => c.DateTimeOffset.Add(DefaultTimeSpan)==WrongDateTimeOffset.Add(DefaultTimeSpan));
      RunWrongTest(c => c.DateTimeOffset.Subtract(DefaultTimeSpan)==WrongDateTimeOffset.Subtract(DefaultTimeSpan));
      RunWrongTest(c => c.DateTimeOffset.Subtract(DateTimeOffsetForSubstract)==WrongDateTimeOffset.Subtract(DateTimeOffsetForSubstract));
      //        RunWrongTest(c => c.DateTimeOffset.ToLocalTime() == WrongDateTimeOffset.ToLocalTime());

      RunWrongTest(c => c.DateTimeOffset + DefaultTimeSpan==WrongDateTimeOffset + DefaultTimeSpan);
      RunWrongTest(c => c.DateTimeOffset - DefaultTimeSpan==WrongDateTimeOffset - DefaultTimeSpan);
      RunWrongTest(c => c.DateTimeOffset - DateTimeForSubstract==WrongDateTimeOffset - DateTimeForSubstract);
      RunWrongTest(c => c.DateTimeOffset - DateTimeOffsetForSubstract==WrongDateTimeOffset - DateTimeOffsetForSubstract);
    }

    #endregion
  }
}
