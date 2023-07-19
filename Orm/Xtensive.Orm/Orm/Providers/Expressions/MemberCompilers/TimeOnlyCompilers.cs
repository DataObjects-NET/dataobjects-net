// Copyright (C) 2022-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers
{
#if NET6_0_OR_GREATER

  [CompilerContainer(typeof(SqlExpression))]
  internal static class TimeOnlyCompilers
  {
    private const long NanosecondsPerTick = 100;

    #region Extractors

    [Compiler(typeof(TimeOnly), "Hour", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyHour(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlTimePart.Hour, _this));

    [Compiler(typeof(TimeOnly), "Minute", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyMinute(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlTimePart.Minute, _this));

    [Compiler(typeof(TimeOnly), "Second", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlySecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlTimePart.Second, _this));

    [Compiler(typeof(TimeOnly), "Millisecond", TargetKind.PropertyGet)]
    public static SqlExpression TimeOnlyMillisecond(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlTimePart.Millisecond, _this));

    [Compiler(typeof(TimeOnly), "Ticks", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTicks(SqlExpression _this) =>
      ExpressionTranslationHelpers.ToLong(SqlDml.TimeToNanoseconds(_this) / NanosecondsPerTick);

    #endregion

    #region Constructors
    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor(
        [Type(typeof(int))] SqlExpression hour,
        [Type(typeof(int))] SqlExpression minute,
        [Type(typeof(int))] SqlExpression second,
        [Type(typeof(int))] SqlExpression millisecond) =>
      SqlDml.TimeConstruct(hour, minute, second, millisecond);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor(
        [Type(typeof(int))] SqlExpression hour,
        [Type(typeof(int))] SqlExpression minute,
        [Type(typeof(int))] SqlExpression second) =>
      SqlDml.TimeConstruct(hour, minute, second, 0);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor(
        [Type(typeof(int))] SqlExpression hour,
        [Type(typeof(int))] SqlExpression minute) =>
      SqlDml.TimeConstruct(hour, minute, 0, 0);

    [Compiler(typeof(TimeOnly), null, TargetKind.Constructor)]
    public static SqlExpression TimeOnlyCtor([Type(typeof(long))] SqlExpression ticks) =>
      SqlDml.TimeConstruct(ticks);

    #endregion

    #region Operators

    [Compiler(typeof(TimeOnly), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorEquality(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 == t2;
    }

    [Compiler(typeof(TimeOnly), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorInequality(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 != t2;
    }

    [Compiler(typeof(TimeOnly), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression TimeOnlyyOperatorGreaterThan(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 > t2;
    }

    [Compiler(typeof(TimeOnly), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorGreaterThanOrEqual(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 >= t2;
    }

    [Compiler(typeof(TimeOnly), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorLessThan(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 < t2;
    }

    [Compiler(typeof(TimeOnly), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorLessThanOrEqual(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return t1 <= t2;
    }

    [Compiler(typeof(TimeOnly), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression TimeOnlyOperatorSubtraction(
      [Type(typeof(TimeOnly))] SqlExpression t1,
      [Type(typeof(TimeOnly))] SqlExpression t2)
    {
      return SqlDml.TimeMinusTime(t1, t2);
    }

    #endregion

    [Compiler(typeof(TimeOnly), "AddHours")]
    public static SqlExpression TimeOnlyAddHours(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeAddHours(_this, value);

    [Compiler(typeof(TimeOnly), "AddMinutes")]
    public static SqlExpression TimeOnlyAddMinutes(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeAddMinutes(_this, value);

    [Compiler(typeof(TimeOnly), "Add")]
    public static SqlExpression TimeOnlyAdd(SqlExpression _this, [Type(typeof(TimeSpan))] SqlExpression value) =>
      SqlDml.TimePlusInterval(_this, value);

    [Compiler(typeof(TimeOnly), "ToString")]
    public static SqlExpression TimeOnlyToStringIso(SqlExpression _this)
    {
      throw new NotSupportedException(Strings.ExTimeOnlyToStringMethodIsNotSupported);
    }

    [Compiler(typeof(TimeOnly), "ToString")]
    public static SqlExpression TimeOnlyToStringIso(SqlExpression _this, [Type(typeof(string))] SqlExpression value)
    {
      var stringValue = value as SqlLiteral<string>;

      if (stringValue == null || !stringValue.Value.Equals("o", StringComparison.OrdinalIgnoreCase))
        throw new NotSupportedException(Strings.ExTranslationOfTimeOnlyToStringWithArbitraryArgumentIsNotSupported);

      return SqlDml.TimeToString(_this);
    }
  }
#endif
}
