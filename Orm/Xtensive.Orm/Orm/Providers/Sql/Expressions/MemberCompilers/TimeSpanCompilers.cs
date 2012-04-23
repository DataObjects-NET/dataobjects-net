// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class TimeSpanCompilers
  {
    private static readonly long TicksPerMillisecond = TimeSpan.FromMilliseconds(1).Ticks;

    private static readonly double MillisecondsPerDay = TimeSpan.FromDays(1).TotalMilliseconds;
    private static readonly double MillisecondsPerHour = TimeSpan.FromHours(1).TotalMilliseconds;
    private static readonly double MillisecondsPerMinute = TimeSpan.FromMinutes(1).TotalMilliseconds;
    private static readonly double MillisecondsPerSecond = TimeSpan.FromSeconds(1).TotalMilliseconds;
    private static readonly double MillisecondsPerTick = TimeSpan.FromTicks(1).TotalMilliseconds;

    private const int NanosecondsPerTick = 100;
    private const int NanosecondsPerMillisecond = 1000000;

    #region Constructors

    internal static SqlExpression GenericIntervalConstruct(
      SqlExpression days,
      SqlExpression hours,
      SqlExpression minutes,
      SqlExpression seconds,
      SqlExpression milliseconds)
    {
      var m = milliseconds + 1000L * (seconds + 60L * (minutes + 60L * (hours + 24L * days)));
      var nanoseconds = NanosecondsPerMillisecond * SqlDml.Cast(m, SqlType.Int64);
      return SqlDml.IntervalConstruct(nanoseconds);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(long))] SqlExpression ticks)
    {
      return SqlDml.IntervalConstruct(ticks * NanosecondsPerTick);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds)
    {
      return GenericIntervalConstruct(0, hours, minutes, seconds, 0);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression days,
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds)
    {
      return GenericIntervalConstruct(days, hours, minutes, seconds, 0);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression days,
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds,
      [Type(typeof(int))] SqlExpression millliseconds)
    {
      return GenericIntervalConstruct(days, hours, minutes, seconds, millliseconds);
    }

    [Compiler(typeof(TimeSpan), "FromDays", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromDays(
      [Type(typeof(double))] SqlExpression days)
    {
      return SqlDml.IntervalConstruct(days * MillisecondsPerDay * NanosecondsPerMillisecond);
    }

    [Compiler(typeof(TimeSpan), "FromHours", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromHours(
      [Type(typeof(double))] SqlExpression hours)
    {
      return SqlDml.IntervalConstruct(hours * MillisecondsPerHour * NanosecondsPerMillisecond);
    }

    [Compiler(typeof(TimeSpan), "FromMinutes", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromMinutes(
      [Type(typeof(double))] SqlExpression minutes)
    {
      return SqlDml.IntervalConstruct(minutes * MillisecondsPerMinute * NanosecondsPerMillisecond);
    }

    [Compiler(typeof(TimeSpan), "FromSeconds", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromSeconds(
      [Type(typeof(double))] SqlExpression seconds)
    {
      return SqlDml.IntervalConstruct(seconds * MillisecondsPerSecond * NanosecondsPerMillisecond);
    }

    [Compiler(typeof(TimeSpan), "FromMilliseconds", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromMilliseconds(
      [Type(typeof(double))] SqlExpression milliseconds)
    {
      return SqlDml.IntervalConstruct(milliseconds * NanosecondsPerMillisecond);
    }

    [Compiler(typeof(TimeSpan), "FromTicks", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression TimeSpanFromTicks(
      [Type(typeof(long))] SqlExpression ticks)
    {
      return SqlDml.IntervalConstruct(ticks * NanosecondsPerTick);
    }

    #endregion

    #region Extractors

    [Compiler(typeof(TimeSpan), "Milliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMilliseconds(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlIntervalPart.Millisecond, _this));
    }

    [Compiler(typeof(TimeSpan), "Seconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanSeconds(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlIntervalPart.Second, _this));
    }

    [Compiler(typeof(TimeSpan), "Minutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMinutes(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlIntervalPart.Minute, _this));
    }

    [Compiler(typeof(TimeSpan), "Hours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanHours(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlIntervalPart.Hour, _this));
    }
    
    [Compiler(typeof(TimeSpan), "Days", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanDays(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlIntervalPart.Day, _this));
    }

    #endregion

    #region Converters

    [Compiler(typeof(TimeSpan), "Ticks", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTicks(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToLong(SqlDml.IntervalToNanoseconds(_this) / NanosecondsPerTick);
    }

    [Compiler(typeof(TimeSpan), "TotalMilliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMilliseconds(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToDouble(SqlDml.IntervalToMilliseconds(_this));
    }

    [Compiler(typeof(TimeSpan), "TotalSeconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalSeconds(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToDouble(SqlDml.IntervalToMilliseconds(_this)) / MillisecondsPerSecond;
    }

    [Compiler(typeof(TimeSpan), "TotalMinutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMinutes(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToDouble(SqlDml.IntervalToMilliseconds(_this)) / MillisecondsPerMinute;
    }

    [Compiler(typeof(TimeSpan), "TotalHours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalHours(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToDouble(SqlDml.IntervalToMilliseconds(_this)) / MillisecondsPerHour;
    }

    [Compiler(typeof(TimeSpan), "TotalDays", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalDays(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToDouble(SqlDml.IntervalToMilliseconds(_this)) / MillisecondsPerDay;
    }

    #endregion

    #region Operators

    [Compiler(typeof(TimeSpan), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorEquality(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1==t2;
    }

    [Compiler(typeof(TimeSpan), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorInequality(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 != t2;
    }

    [Compiler(typeof(TimeSpan), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorGreaterThan(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 > t2;
    }

    [Compiler(typeof(TimeSpan), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorGreaterThanOrEqual(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 >= t2;
    }

    [Compiler(typeof(TimeSpan), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorLessThan(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 < t2;
    }

    [Compiler(typeof(TimeSpan), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorLessThanOrEqual(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 <= t2;
    }

    [Compiler(typeof(TimeSpan), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorAddition(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 + t2;
    }

    [Compiler(typeof(TimeSpan), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorSubtraction(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      return t1 - t2;
    }

    [Compiler(typeof(TimeSpan), Operator.UnaryPlus, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorUnaryPlus(
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return t;
    }

    [Compiler(typeof(TimeSpan), Operator.UnaryNegation, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorUnaryNegation(
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return -t;
    }

    #endregion

    #region Other mappings

    [Compiler(typeof(TimeSpan), "Add")]
    public static SqlExpression TimeSpanAdd(SqlExpression _this,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return _this + t;
    }

    [Compiler(typeof(TimeSpan), "Subtract")]
    public static SqlExpression TimeSpanSubtract(SqlExpression _this,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return _this - t;
    }

    [Compiler(typeof(TimeSpan), "Negate")]
    public static SqlExpression TimeSpanNegate(SqlExpression _this)
    {
      return SqlDml.IntervalNegate(_this);
    }
    
    [Compiler(typeof(TimeSpan), "Duration")]
    public static SqlExpression TimeSpanDuration(SqlExpression _this)
    {
      return SqlDml.IntervalAbs(_this);
    }

    #endregion
  }
}