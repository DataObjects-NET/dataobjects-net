// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.11.25

using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof (SqlExpression))]
  internal static class DateTimeOffsetCompilers
  {
    #region Extractors

    [Compiler(typeof (DateTimeOffset), "Year", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetYear(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Year, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Month", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetMonth(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Month, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Day", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetDay(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Day, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetHour(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetMinute(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Second", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetSecond(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Second, _this));
    }

    [Compiler(typeof (DateTimeOffset), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetMillisecond(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, _this));
    }

    [Compiler(typeof (DateTimeOffset), "TimeOfDay", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression _this)
    {
      return SqlDml.DateTimeOffsetTimeOfDay(_this);
    }

    [Compiler(typeof (DateTimeOffset), "Date", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetDate(SqlExpression _this)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.Date, _this);
    }

    [Compiler(typeof (DateTimeOffset), "DayOfWeek", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetDayOfWeek(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfWeek, _this));
    }

    [Compiler(typeof (DateTimeOffset), "DayOfYear", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetDayOfYear(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfYear, _this));
    }

    [Compiler(typeof (DateTimeOffset), "DateTime", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetDateTime(SqlExpression _this)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, _this);
    }

    [Compiler(typeof(DateTimeOffset), "ToLocalTime")]
    public static SqlExpression DateTimeOffsetToLocalTime(SqlExpression _this)
    {
      return SqlDml.DateTimeOffsetToLocalTime(_this);
    }

    [Compiler(typeof (DateTimeOffset), "Offset", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetOffset(SqlExpression _this)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.Offset, _this);
    }

    [Compiler(typeof (DateTimeOffset), "UtcDateTime", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetUtcDateTime(SqlExpression _this)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, _this);
    }

    [Compiler(typeof (DateTimeOffset), "LocalDateTime", TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression _this)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.LocalDateTime, _this);
    }

    [Compiler(typeof(DateTimeOffset), "ToUniversalTime")]
    public static SqlExpression DateTimeOffsetToUtcTime(SqlExpression _this)
    {
      return SqlDml.DateTimeOffsetToUtcTime(_this);
    }

    #endregion

    #region Constructors

    private static SqlExpression DateTimeOffsetConstruct(
      SqlExpression year,
      SqlExpression month,
      SqlExpression day,
      SqlExpression hour,
      SqlExpression minute,
      SqlExpression second,
      SqlExpression millisecond,
      SqlExpression offsetInMinutes)
    {
      return SqlDml.DateTimeOffsetPlusInterval(
        SqlDml.DateTimeOffsetConstruct(year, month, day, offsetInMinutes),
        TimeSpanCompilers.GenericIntervalConstruct(0, hour, minute, second, millisecond));
    }

    [Compiler(typeof (DateTimeOffset), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeOffsetCtor(
      [Type(typeof (DateTime))] SqlExpression dateTime)
    {
      return SqlDml.DateTimeToDateTimeOffset(dateTime);
    }

    [Compiler(typeof (DateTimeOffset), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeOffsetCtor(
      [Type(typeof (DateTime))] SqlExpression dateTime,
      [Type(typeof (TimeSpan))] SqlExpression offset)
    {
      SqlExpression offsetInMinutes = OffsetInMinutes(offset);

      return DateTimeOffsetConstruct(
        DateTimeCompilers.DateTimeYear(dateTime),
        DateTimeCompilers.DateTimeMonth(dateTime),
        DateTimeCompilers.DateTimeDay(dateTime),
        DateTimeCompilers.DateTimeHour(dateTime),
        DateTimeCompilers.DateTimeMinute(dateTime),
        DateTimeCompilers.DateTimeSecond(dateTime),
        DateTimeCompilers.DateTimeMillisecond(dateTime),
        offsetInMinutes);
    }

    [Compiler(typeof (DateTimeOffset), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeOffsetCtor(
      [Type(typeof (int))] SqlExpression year,
      [Type(typeof (int))] SqlExpression month,
      [Type(typeof (int))] SqlExpression day,
      [Type(typeof (int))] SqlExpression hour,
      [Type(typeof (int))] SqlExpression minute,
      [Type(typeof (int))] SqlExpression second,
      [Type(typeof (TimeSpan))] SqlExpression offset)
    {
      SqlExpression offsetInMinutes = OffsetInMinutes(offset);

      return DateTimeOffsetConstruct(
        year,
        month,
        day,
        hour,
        minute,
        second,
        0L,
        offsetInMinutes);
    }

    [Compiler(typeof (DateTimeOffset), null, TargetKind.Constructor)]
    public static SqlExpression DateTimeOffsetCtor(
      [Type(typeof (int))] SqlExpression year,
      [Type(typeof (int))] SqlExpression month,
      [Type(typeof (int))] SqlExpression day,
      [Type(typeof (int))] SqlExpression hour,
      [Type(typeof (int))] SqlExpression minute,
      [Type(typeof (int))] SqlExpression second,
      [Type(typeof (int))] SqlExpression millisecond,
      [Type(typeof (TimeSpan))] SqlExpression offset)
    {
      SqlExpression offsetInMinutes = OffsetInMinutes(offset);

      return DateTimeOffsetConstruct(
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        offsetInMinutes);
    }

    #endregion

    private static SqlExpression OffsetInMinutes(SqlExpression offset)
    {
      return TimeSpanCompilers.TimeSpanTotalMinutes(offset);
    }

    #region Operators

    [Compiler(typeof (DateTimeOffset), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorEquality(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left==right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorInequality(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left!=right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorGreaterThan(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left > right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorGreaterThanOrEqual(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left >= right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorLessThan(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left < right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorLessThanOrEqual(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return left <= right;
    }

    [Compiler(typeof (DateTimeOffset), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorAddition(
      [Type(typeof (DateTimeOffset))] SqlExpression dateTimeTz,
      [Type(typeof (TimeSpan))] SqlExpression timeSpan)
    {
      return SqlDml.DateTimeOffsetPlusInterval(dateTimeTz, timeSpan);
    }

    [Compiler(typeof (DateTimeOffset), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorSubtractionTimeSpan(
      [Type(typeof (DateTimeOffset))] SqlExpression dateTimeTz,
      [Type(typeof (TimeSpan))] SqlExpression timeSpan)
    {
      return SqlDml.DateTimeOffsetMinusInterval(dateTimeTz, timeSpan);
    }

    [Compiler(typeof (DateTimeOffset), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOffsetOperatorSubtractionDateTimeOffset(
      [Type(typeof (DateTimeOffset))] SqlExpression left,
      [Type(typeof (DateTimeOffset))] SqlExpression right)
    {
      return SqlDml.DateTimeOffsetMinusDateTimeOffset(left, right);
    }

    #endregion

    [Compiler(typeof (DateTimeOffset), "Add")]
    public static SqlExpression DateTimeOffsetAdd(SqlExpression _this,
      [Type(typeof (TimeSpan))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, value);
    }

    [Compiler(typeof (DateTimeOffset), "AddYears")]
    public static SqlExpression DateTimeOffsetAddYears(SqlExpression _this,
      [Type(typeof (int))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetAddYears(_this, value);
    }

    [Compiler(typeof (DateTimeOffset), "AddMonths")]
    public static SqlExpression DateTimeOffsetAddMonths(SqlExpression _this,
      [Type(typeof (int))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetAddMonths(_this, value);
    }

    [Compiler(typeof (DateTimeOffset), "AddDays")]
    public static SqlExpression DateTimeOffsetAddDays(SqlExpression _this,
      [Type(typeof (double))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, TimeSpanCompilers.TimeSpanFromDays(value));
    }

    [Compiler(typeof (DateTimeOffset), "AddHours")]
    public static SqlExpression DateTimeOffsetAddHours(SqlExpression _this,
      [Type(typeof (double))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, TimeSpanCompilers.TimeSpanFromHours(value));
    }

    [Compiler(typeof (DateTimeOffset), "AddMinutes")]
    public static SqlExpression DateTimeOffsetAddMinutes(SqlExpression _this,
      [Type(typeof (double))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, TimeSpanCompilers.TimeSpanFromMinutes(value));
    }

    [Compiler(typeof (DateTimeOffset), "AddSeconds")]
    public static SqlExpression DateTimeOffsetAddSeconds(SqlExpression _this,
      [Type(typeof (double))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, TimeSpanCompilers.TimeSpanFromSeconds(value));
    }

    [Compiler(typeof (DateTimeOffset), "AddMilliseconds")]
    public static SqlExpression DateTimeOffsetAddMilliseconds(SqlExpression _this,
      [Type(typeof (double))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetPlusInterval(_this, TimeSpanCompilers.TimeSpanFromMilliseconds(value));
    }

    [Compiler(typeof (DateTimeOffset), "Subtract")]
    public static SqlExpression DateTimeOffsetSubtractTimeSpan(SqlExpression _this,
      [Type(typeof (TimeSpan))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetMinusInterval(_this, value);
    }

    [Compiler(typeof (DateTimeOffset), "Subtract")]
    public static SqlExpression DateTimeOffsetSubtractDateTimeOffset(SqlExpression _this,
      [Type(typeof (DateTimeOffset))] SqlExpression value)
    {
      return SqlDml.DateTimeOffsetMinusDateTimeOffset(_this, value);
    }

    [Compiler(typeof (DateTimeOffset), "Now", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeOffsetNow()
    {
      return SqlDml.CurrentDateTimeOffset();
    }
  }
}