// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class DateTimeMappings
  {
    #region Extractors

    [Compiler(typeof(DateTime), "Year", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeYear(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Year, _this));
    }

    [Compiler(typeof(DateTime), "Month", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMonth(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Month, _this));
    }

    [Compiler(typeof(DateTime), "Day", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDay(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Day, _this));
    }

    [Compiler(typeof(DateTime), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeHour(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Hour, _this));
    }

    [Compiler(typeof(DateTime), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMinute(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Minute, _this));
    }

    [Compiler(typeof(DateTime), "Second", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeSecond(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Second, _this));
    }

    [Compiler(typeof(DateTime), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMillisecond(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.Millisecond, _this));
    }

    [Compiler(typeof(DateTime), "TimeOfDay", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeTimeOfDay(SqlExpression _this)
    {
      return SqlFactory.DateTimeSubtractDateTime(_this, SqlFactory.DateTimeTruncate(_this));
    }

    [Compiler(typeof(DateTime), "Date", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDate(SqlExpression _this)
    {
      return SqlFactory.DateTimeTruncate(_this);
    }

    [Compiler(typeof(DateTime), "DayOfWeek", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfWeek(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.DayOfWeek, _this));
    }

    [Compiler(typeof(DateTime), "DayOfYear", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfYear(SqlExpression _this)
    {
      return ToInt(SqlFactory.Extract(SqlDateTimePart.DayOfYear, _this));
    }

    #endregion

    #region Constructors

    private static SqlExpression DateTimeConstruct(
      SqlExpression year,
      SqlExpression month,
      SqlExpression day,
      SqlExpression hour,
      SqlExpression minute,
      SqlExpression second,
      SqlExpression millisecond)
    {
      return SqlFactory.DateTimeAddInterval(
        SqlFactory.DateTimeConstruct(year, month, day),
        TimeSpanMappings.IntervalConstruct(0, hour, minute, second, millisecond)
        );
    }

    [Compiler(typeof(DateTime), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeCtor(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month, 
      [Type(typeof(int))] SqlExpression day)
    {
      return SqlFactory.DateTimeConstruct(year, month, day);
    }

    [Compiler(typeof(DateTime), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeCtor(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month,
      [Type(typeof(int))] SqlExpression day,
      [Type(typeof(int))] SqlExpression hour,
      [Type(typeof(int))] SqlExpression minute,
      [Type(typeof(int))] SqlExpression second)
    {
      return DateTimeConstruct(year, month, day, hour, minute, second, 0L);
    }

    [Compiler(typeof(DateTime), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeCtor(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month,
      [Type(typeof(int))] SqlExpression day,
      [Type(typeof(int))] SqlExpression hour,
      [Type(typeof(int))] SqlExpression minute,
      [Type(typeof(int))] SqlExpression second,
      [Type(typeof(int))] SqlExpression millisecond)
    {
      return DateTimeConstruct(year, month, day, hour, minute, second, millisecond);
    }

    #endregion

    #region Operators

    [Compiler(typeof(DateTime), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorEquality(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 == d2;
    }

    [Compiler(typeof(DateTime), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorInequality(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 != d2;
    }

    [Compiler(typeof(DateTime), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorGreaterThan(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 > d2;
    }

    [Compiler(typeof(DateTime), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorGreaterThanOrEqual(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 >= d2;
    }

    [Compiler(typeof(DateTime), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorLessThan(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 < d2;
    }

    [Compiler(typeof(DateTime), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorLessThanOrEqual(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return d1 <= d2;
    }


    [Compiler(typeof(DateTime), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorAddition(
      [Type(typeof(DateTime))] SqlExpression d,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return SqlFactory.DateTimeAddInterval(d, t);
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionTimeSpan(
      [Type(typeof(DateTime))] SqlExpression d,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return SqlFactory.DateTimeSubtractInterval(d, t);
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionDateTime(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return SqlFactory.DateTimeSubtractDateTime(d1, d2);
    }

    #endregion

    [Compiler(typeof(DateTime), "Add")]
    public static SqlExpression DateTimeAdd(SqlExpression _this,
      [Type(typeof(TimeSpan))] SqlExpression value)
    {
      return SqlFactory.DateTimeAddInterval(_this, value);
    }

    [Compiler(typeof(DateTime), "Subtract")]
    public static SqlExpression DateTimeSubtractTimeSpan(SqlExpression _this,
      [Type(typeof(TimeSpan))] SqlExpression value)
    {
      return SqlFactory.DateTimeSubtractInterval(_this, value);
    }

    [Compiler(typeof(DateTime), "Subtract")]
    public static SqlExpression DateTimeSubtractDateTime(SqlExpression _this,
      [Type(typeof(DateTime))] SqlExpression value)
    {
      return SqlFactory.DateTimeSubtractDateTime(_this, value);
    }

    [Compiler(typeof(DateTime), "Now", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeNow()
    {
      return SqlFactory.CurrentTimeStamp();
    }

    [Compiler(typeof(DateTime), "Today", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeToday()
    {
      return SqlFactory.CurrentDate();
    }

    [Compiler(typeof(DateTime), "IsLeapYear", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DateTimeIsLeapYear([Type(typeof(int))] SqlExpression year)
    {
      return ((year % 4==0) && (year % 100!=0)) || (year % 400==0);
    }

    [Compiler(typeof(DateTime), "DaysInMonth", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DateTimeDaysInMonth(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month)
    {
      var februaryCase = SqlFactory.Case();
      februaryCase.Add(DateTimeIsLeapYear(year), 29);
      februaryCase.Else = 28;

      var result = SqlFactory.Case();
      result.Add(SqlFactory.In(month, SqlFactory.Array(1, 3, 5, 7, 8, 10, 12)), 31);
      result.Add(month==2, februaryCase);
      result.Else = 30;

      return result;
    }

    private static SqlExpression ToInt(SqlExpression target)
    {
      return SqlFactory.Cast(target, SqlDataType.Int32);
    }
  }
}
