// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class DateTimeCompilers
  {
    #region Extractors

    [Compiler(typeof(DateTime), "Year", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeYear(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Year, _this));

    [Compiler(typeof(DateTime), "Month", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMonth(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Month, _this));

    [Compiler(typeof(DateTime), "Day", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDay(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Day, _this));

    [Compiler(typeof(DateTime), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeHour(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Hour, _this));

    [Compiler(typeof(DateTime), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMinute(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Minute, _this));

    [Compiler(typeof(DateTime), "Second", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeSecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Second, _this));

    [Compiler(typeof(DateTime), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeMillisecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Millisecond, _this));

    [Compiler(typeof(DateTime), "TimeOfDay", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeTimeOfDay(SqlExpression _this) =>
      SqlDml.DateTimeMinusDateTime(_this, SqlDml.DateTimeTruncate(_this));

    [Compiler(typeof(DateTime), "Date", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDate(SqlExpression _this) =>
      SqlDml.DateTimeTruncate(_this);

    [Compiler(typeof(DateTime), "DayOfWeek", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfWeek(SqlExpression _this)
    {
      var baseExpression = ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.DayOfWeek, _this));
      var context = ExpressionTranslationContext.Current;
      if (context == null) {
        return baseExpression;
      }
      if (context.ProviderInfo.ProviderName == WellKnown.Provider.MySql) {
        return baseExpression - 1; //Mysql starts days of week from 1 unlike in .Net.
      }
      return baseExpression;
    }

    [Compiler(typeof(DateTime), "DayOfYear", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeDayOfYear(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.DayOfYear, _this));
    }

#if DO_DATEONLY
    [Compiler(typeof(DateOnly), "Year", TargetKind.PropertyGet)]
    public static SqlExpression DateOnlyYear(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Year, _this));

    [Compiler(typeof(DateOnly), "Month", TargetKind.PropertyGet)]
    public static SqlExpression DateOnlyMonth(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Month, _this));

    [Compiler(typeof(DateOnly), "Day", TargetKind.PropertyGet)]
    public static SqlExpression DateOnlyDay(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Day, _this));

    [Compiler(typeof(TimeOnly), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyHour(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Hour, _this));

    [Compiler(typeof(TimeOnly), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyMinute(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Minute, _this));

    [Compiler(typeof(TimeOnly), "Second", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlySecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Second, _this));

    [Compiler(typeof(TimeOnly), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyMillisecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Millisecond, _this));
#endif // DO_DATEONLY

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
      return SqlDml.DateTimePlusInterval(
        SqlDml.DateTimeConstruct(year, month, day),
        TimeSpanCompilers.GenericIntervalConstruct(0, hour, minute, second, millisecond));
    }

    [Compiler(typeof(DateTime), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeCtor(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month,
      [Type(typeof(int))] SqlExpression day)
    {
      return SqlDml.DateTimeConstruct(year, month, day);
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

#if DO_DATEONLY
    [Compiler(typeof(DateOnly), null, TargetKind.Constructor)]
    public static SqlExpression DateOnlyCtor(
        [Type(typeof(int))] SqlExpression year,
        [Type(typeof(int))] SqlExpression month,
        [Type(typeof(int))] SqlExpression day) =>
      SqlDml.DateOnlyConstruct(year, month, day);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor(
        [Type(typeof(int))] SqlExpression hour,
        [Type(typeof(int))] SqlExpression minute,
        [Type(typeof(int))] SqlExpression second) =>
      SqlDml.TimeOnlyConstruct(hour, minute, second, 0);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor(
        [Type(typeof(int))] SqlExpression hour,
        [Type(typeof(int))] SqlExpression minute) =>
      SqlDml.TimeOnlyConstruct(hour, minute, 0, 0);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor([Type(typeof(long))] SqlExpression ticks) =>
      new SqlFunctionCall(SqlFunctionType.TimeOnlyConstruct, ticks);
#endif // DO_DATEONLY

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
      return SqlDml.DateTimePlusInterval(d, t);
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionTimeSpan(
      [Type(typeof(DateTime))] SqlExpression d,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return SqlDml.DateTimeMinusInterval(d, t);
    }

    [Compiler(typeof(DateTime), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionDateTime(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      return SqlDml.DateTimeMinusDateTime(d1, d2);
    }

#if DO_DATEONLY
    [Compiler(typeof(DateOnly), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorEquality(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 == d2;
    }

    [Compiler(typeof(DateOnly), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorInequality(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 != d2;
    }

    [Compiler(typeof(DateOnly), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorGreaterThan(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 > d2;
    }

    [Compiler(typeof(DateOnly), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorGreaterThanOrEqual(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 >= d2;
    }

    [Compiler(typeof(DateOnly), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorLessThan(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 < d2;
    }

    [Compiler(typeof(DateOnly), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateOnlyOperatorLessThanOrEqual(
      [Type(typeof(DateOnly))] SqlExpression d1,
      [Type(typeof(DateOnly))] SqlExpression d2)
    {
      return d1 <= d2;
    }

    [Compiler(typeof(TimeOnly), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorEquality(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 == d2;
    }

    [Compiler(typeof(TimeOnly), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorInequality(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 != d2;
    }

    [Compiler(typeof(TimeOnly), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression TimeOnlyyOperatorGreaterThan(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 > d2;
    }

    [Compiler(typeof(TimeOnly), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorGreaterThanOrEqual(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 >= d2;
    }

    [Compiler(typeof(TimeOnly), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorLessThan(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 < d2;
    }

    [Compiler(typeof(TimeOnly), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorLessThanOrEqual(
      [Type(typeof(TimeOnly))] SqlExpression d1,
      [Type(typeof(TimeOnly))] SqlExpression d2)
    {
      return d1 <= d2;
    }

#endif // DO_DATEONLY

    #endregion

    [Compiler(typeof(DateTime), "Add")]
    public static SqlExpression DateTimeAdd(SqlExpression _this, [Type(typeof(TimeSpan))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, value);

    [Compiler(typeof(DateTime), "AddYears")]
    public static SqlExpression DateTimeAddYears(SqlExpression _this, [Type(typeof(int))] SqlExpression value) =>
      SqlDml.DateTimeAddYears(_this, value);

    [Compiler(typeof(DateTime), "AddMonths")]
    public static SqlExpression DateTimeAddMonths(SqlExpression _this, [Type(typeof(int))] SqlExpression value) =>
      SqlDml.DateTimeAddMonths(_this, value);

    [Compiler(typeof(DateTime), "AddDays")]
    public static SqlExpression DateTimeAddDays(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, TimeSpanCompilers.TimeSpanFromDays(value));

    [Compiler(typeof(DateTime), "AddHours")]
    public static SqlExpression DateTimeAddHours(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, TimeSpanCompilers.TimeSpanFromHours(value));

    [Compiler(typeof(DateTime), "AddMinutes")]
    public static SqlExpression DateTimeAddMinutes(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, TimeSpanCompilers.TimeSpanFromMinutes(value));

    [Compiler(typeof(DateTime), "AddSeconds")]
    public static SqlExpression DateTimeAddSeconds(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, TimeSpanCompilers.TimeSpanFromSeconds(value));

    [Compiler(typeof(DateTime), "AddMilliseconds")]
    public static SqlExpression DateTimeAddMilliseconds(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.DateTimePlusInterval(_this, TimeSpanCompilers.TimeSpanFromMilliseconds(value));

    [Compiler(typeof(DateTime), "Subtract")]
    public static SqlExpression DateTimeSubtractTimeSpan(SqlExpression _this, [Type(typeof(TimeSpan))] SqlExpression value) =>
      SqlDml.DateTimeMinusInterval(_this, value);

    [Compiler(typeof(DateTime), "Subtract")]
    public static SqlExpression DateTimeSubtractDateTime(SqlExpression _this, [Type(typeof(DateTime))] SqlExpression value) =>
      SqlDml.DateTimeMinusDateTime(_this, value);

    [Compiler(typeof(DateTime), "Now", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeNow() =>
      SqlDml.CurrentTimeStamp();

    [Compiler(typeof(DateTime), "Today", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeToday() =>
      SqlDml.CurrentDate();

    [Compiler(typeof(DateTime), "IsLeapYear", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DateTimeIsLeapYear([Type(typeof(int))] SqlExpression year) =>
      ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);

    [Compiler(typeof(DateTime), "DaysInMonth", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DateTimeDaysInMonth(
      [Type(typeof(int))] SqlExpression year,
      [Type(typeof(int))] SqlExpression month)
    {
      var februaryCase = SqlDml.Case();
      februaryCase.Add(DateTimeIsLeapYear(year), 29);
      februaryCase.Else = 28;

      var result = SqlDml.Case();
      result.Add(SqlDml.In(month, SqlDml.Array(1, 3, 5, 7, 8, 10, 12)), 31);
      result.Add(month == 2, februaryCase);
      result.Else = 30;

      return result;
    }

    [Compiler(typeof(DateTime), "ToString")]
    public static SqlExpression DateTimeToStringIso(SqlExpression _this)
    {
      throw new NotSupportedException(Strings.ExDateTimeToStringMethodIsNotSupported);
    }

    [Compiler(typeof(DateTime), "ToString")]
    public static SqlExpression DateTimeToStringIso(SqlExpression _this, [Type(typeof(string))] SqlExpression value)
    {
      var stringValue = value as SqlLiteral<string>;

      if (stringValue == null)
        throw new NotSupportedException(Strings.ExTranslationOfDateTimeToStringWithArbitraryArgumentsIsNotSupported);

      if (!stringValue.Value.Equals("s"))
        throw new NotSupportedException(Strings.ExTranslationOfDateTimeToStringWithArbitraryArgumentsIsNotSupported);

      return SqlDml.DateTimeToStringIso(_this);
    }

#if DO_DATEONLY
    [Compiler(typeof(DateOnly), "AddYears")]
    public static SqlExpression DateOnlyAddYears(SqlExpression _this, [Type(typeof(int))] SqlExpression value) =>
      SqlDml.DateTimeAddYears(_this, value);

    [Compiler(typeof(DateOnly), "AddMonths")]
    public static SqlExpression DateOnlyAddMonths(SqlExpression _this, [Type(typeof(int))] SqlExpression value) =>
      SqlDml.DateTimeAddMonths(_this, value);

    [Compiler(typeof(DateOnly), "AddDays")]
    public static SqlExpression DateOnlyAddDays(SqlExpression _this, [Type(typeof(int))] SqlExpression value) =>
      SqlDml.DateOnlyAddDays(_this, value);

    [Compiler(typeof(TimeOnly), "AddHours")]
    public static SqlExpression TimeOnlyAddHours(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeOnlyAddHours(_this, value);

    [Compiler(typeof(TimeOnly), "AddMinutes")]
    public static SqlExpression TimeOnlyAddMinutes(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeOnlyAddMinutes(_this, value);

#endif // DO_DATEONLY
  }
}
