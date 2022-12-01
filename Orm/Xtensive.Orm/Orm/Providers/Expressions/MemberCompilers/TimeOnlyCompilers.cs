// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.Expressions.MemberCompilers
{
#if NET6_0_OR_GREATER

  [CompilerContainer(typeof(SqlExpression))]
  internal static class TimeOnlyCompilers
  {
    #region Extractors

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

    #endregion

    #region Constructors

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
      new SqlFunctionCall(SqlFunctionType.TimeConstruct, ticks);

    #endregion

    #region Operators

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

    [Compiler(typeof(TimeOnly), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DateTimeOperatorSubtractionDateTime(
      [Type(typeof(DateTime))] SqlExpression d1,
      [Type(typeof(DateTime))] SqlExpression d2)
    {
      throw new NotImplementedException();
      //return SqlDml.DateTimeMinusDateTime(d1, d2);
    }

    #endregion

    [Compiler(typeof(TimeOnly), "AddHours")]
    public static SqlExpression TimeOnlyAddHours(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeAddHours(_this, value);

    [Compiler(typeof(TimeOnly), "AddMinutes")]
    public static SqlExpression TimeOnlyAddMinutes(SqlExpression _this, [Type(typeof(double))] SqlExpression value) =>
      SqlDml.TimeAddMinutes(_this, value);

    [Compiler(typeof(TimeOnly), "ToString")]
    public static SqlExpression TimeOnlyToStringIso(SqlExpression _this)
    {
      throw new NotSupportedException(Strings.ExDateTimeToStringMethodIsNotSupported);
    }

    [Compiler(typeof(TimeOnly), "ToString")]
    public static SqlExpression TimeOnlyToStringIso(SqlExpression _this, [Type(typeof(string))] SqlExpression value)
    {
      throw new NotImplementedException();

      //var stringValue = value as SqlLiteral<string>;

      //if (stringValue == null)
      //  throw new NotSupportedException(Strings.ExTranslationOfDateTimeToStringWithArbitraryArgumentsIsNotSupported);

      //if (!stringValue.Value.Equals("s"))
      //  throw new NotSupportedException(Strings.ExTranslationOfDateTimeToStringWithArbitraryArgumentsIsNotSupported);

      //return SqlDml.DateTimeToStringIso(_this);
    }
  }
#endif
}
