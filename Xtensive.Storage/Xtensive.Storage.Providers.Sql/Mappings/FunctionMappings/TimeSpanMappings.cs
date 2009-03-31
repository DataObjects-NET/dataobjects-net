// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class TimeSpanMappings
  {
    #region Constructors

    internal static SqlExpression IntervalConstruct(
      SqlExpression days,
      SqlExpression hours,
      SqlExpression minutes,
      SqlExpression seconds,
      SqlExpression milliseconds)
    {
      // to be optimized
      return SqlFactory.IntervalConstruct(
        milliseconds + 1000L * (seconds + 60L * (minutes + 60L * (hours + 24L * days)))
        );
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(long))] SqlExpression ticks)
    {
      return SqlFactory.IntervalConstruct(ticks / 10000);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds)
    {
      return IntervalConstruct(0, hours, minutes, seconds, 0);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression days,
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds)
    {
      return IntervalConstruct(days, hours, minutes, seconds, 0);
    }

    [Compiler(typeof(TimeSpan), null, TargetKind.Constructor)]
    public static SqlExpression TimeSpanCtor(
      [Type(typeof(int))] SqlExpression days,
      [Type(typeof(int))] SqlExpression hours,
      [Type(typeof(int))] SqlExpression minutes,
      [Type(typeof(int))] SqlExpression seconds,
      [Type(typeof(int))] SqlExpression millliseconds)
    {
      return IntervalConstruct(days, hours, minutes, seconds, millliseconds);
    }

    #endregion

    #region Extractors

    [Compiler(typeof(TimeSpan), "Milliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMilliseconds(SqlExpression this_)
    {
      return SqlFactory.IntervalExtract(SqlIntervalPart.Millisecond, this_);
    }

    [Compiler(typeof(TimeSpan), "Seconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanSeconds(SqlExpression this_)
    {
      return SqlFactory.IntervalExtract(SqlIntervalPart.Second, this_);
    }

    [Compiler(typeof(TimeSpan), "Minutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMinutes(SqlExpression this_)
    {
      return SqlFactory.IntervalExtract(SqlIntervalPart.Minute, this_);
    }

    [Compiler(typeof(TimeSpan), "Hours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanHours(SqlExpression this_)
    {
      return SqlFactory.IntervalExtract(SqlIntervalPart.Hour, this_);
    }
    
    [Compiler(typeof(TimeSpan), "Days", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanDays(SqlExpression this_)
    {
      return SqlFactory.IntervalExtract(SqlIntervalPart.Day, this_);
    }

    #endregion

    #region Converters

    [Compiler(typeof(TimeSpan), "Ticks", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTicks(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_) * 10000;
    }

    [Compiler(typeof(TimeSpan), "TotalMilliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMilliseconds(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_);
    }

    [Compiler(typeof(TimeSpan), "TotalSeconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalSeconds(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_) / 1000.0;
    }

    [Compiler(typeof(TimeSpan), "TotalMinutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMinutes(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_) / (1000.0 * 60.0);
    }

    [Compiler(typeof(TimeSpan), "TotalHours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalHours(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_) / (1000.0 * 60.0 * 60.0);
    }

    [Compiler(typeof(TimeSpan), "TotalDays", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalDays(SqlExpression this_)
    {
      return SqlFactory.IntervalToMilliseconds(this_) / (1000.0 * 60.0 * 60.0 * 24.0);
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
    public static SqlExpression TimeSpanAdd(SqlExpression this_,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return this_ + t;
    }

    [Compiler(typeof(TimeSpan), "Subtract")]
    public static SqlExpression TimeSpanSubtract(SqlExpression this_,
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return this_ - t;
    }

    [Compiler(typeof(TimeSpan), "Negate")]
    public static SqlExpression TimeSpanNegate(SqlExpression this_)
    {
      return -this_;
    }
    
    [Compiler(typeof(TimeSpan), "Duration")]
    public static SqlExpression TimeSpanDuration(SqlExpression this_)
    {
      return SqlFactory.IntervalDuration(this_);
    }

    #endregion
  }
}
