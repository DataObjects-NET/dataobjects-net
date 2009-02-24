// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class DateTimeMappings
  {
    #region Extractors

    [Compiler(typeof(DateTime), "Year", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeYear(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Year, this_);
    }

    [Compiler(typeof(DateTime), "Month", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMonth(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Month, this_);
    }

    [Compiler(typeof(DateTime), "Day", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDay(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Day, this_);
    }

    [Compiler(typeof(DateTime), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeHour(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Hour, this_);
    }

    [Compiler(typeof(DateTime), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMinute(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Minute, this_);
    }

    [Compiler(typeof(DateTime), "Second", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeSecond(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Second, this_);
    }

    [Compiler(typeof(DateTime), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMillisecond(SqlExpression this_)
    {
      return SqlFactory.Extract(SqlDateTimePart.Millisecond, this_);
    }

    [Compiler(typeof(DateTime), "TimeOfDay", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeTimeOfDay(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(DateTime), "Date", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDate(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(DateTime), "DayOfWeek", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfWeek(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(DateTime), "DayOfYear", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfYear(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Constructors

    [Compiler(typeof(DateTime), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeCtor(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month, 
      [Type(typeof(int))] SqlExpression day)
    {
      throw new NotImplementedException();
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
      throw new NotImplementedException();
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
      throw new NotImplementedException();
    }

    #endregion

    #region Operators

    [Compiler(typeof(DateTime), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorAddition(
      [Type(typeof(DateTime))] SqlExpression d,
      [Type(typeof(TimeSpan))] SqlExpression s)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionTimeSpan(
      [Type(typeof(DateTime))] SqlExpression d,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionDateTime(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      throw new NotImplementedException();
    }

    #endregion

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
      februaryCase.Add(DateTimeIsLeapYear(year), SqlFactory.Literal(29));
      februaryCase.Else = SqlFactory.Literal(28);

      var result = SqlFactory.Case();
      result.Add(SqlFactory.In(month, SqlFactory.Array(1, 3, 7, 8, 10, 12)), SqlFactory.Literal(31));
      result.Add(month==2, februaryCase);
      result.Else = SqlFactory.Literal(30);

      return result;
    }
  }
}
