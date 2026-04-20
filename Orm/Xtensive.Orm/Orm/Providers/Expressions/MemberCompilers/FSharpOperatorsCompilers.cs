// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Reflection;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
#pragma warning disable IDE0060 // Remove unused parameter
  [CompilerContainer(typeof(SqlExpression))]
  internal static class FSharpOperatorsCompilers
  {
#if NET8_0_OR_GREATER
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    private const string FSharpLanguagePrimitives = "Microsoft.FSharp.Core.LanguagePrimitives, FSharp.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif NET7_0
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    private const string FSharpLanguagePrimitives = "Microsoft.FSharp.Core.LanguagePrimitives, FSharp.Core, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    private const string FSharpLanguagePrimitives = "Microsoft.FSharp.Core.LanguagePrimitives, FSharp.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#endif

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Addition, TargetKind.Operator, 3)]
    public static SqlExpression OperationAddition(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.Add(left, right);
    }

    [Compiler(FSharpLanguagePrimitives, "AdditionDynamic", TargetKind.Static | TargetKind.Method, 3)]
    public static SqlExpression DynamicAddition(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      var method = member as MethodInfo;
      var arguments = method.GetGenericArguments();
      var firstArgument = arguments[0];
      if (firstArgument == WellKnownTypes.DateTime)
        return DateTimeCompilers.DateTimeOperatorAddition(left, right);
      if (firstArgument == WellKnownTypes.DateTimeOffset)
        return DateTimeOffsetCompilers.DateTimeOffsetOperatorAddition(left, right);
      if (firstArgument == WellKnownTypes.Decimal)
        return DecimalCompilers.DecimalOperatorAddition(left, right);
      if (firstArgument == WellKnownTypes.TimeSpan)
        return TimeSpanCompilers.TimeSpanOperatorAddition(left, right);

      return OperationAddition(member, left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Subtraction, TargetKind.Operator, 3)]
    public static SqlExpression OperationSubtraction(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.Subtract(left, right);
    }

    [Compiler(FSharpLanguagePrimitives, "SubtractionDynamic", TargetKind.Static | TargetKind.Method, 3)]
    public static SqlExpression DynamicSubtraction(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      var method = member as MethodInfo;
      var arguments = method.GetGenericArguments();
      var firstArgument = arguments[0];
      if (firstArgument == WellKnownTypes.DateTime) {
        if (arguments[1] == WellKnownTypes.DateTime)
          return DateTimeCompilers.DateTimeOperatorSubtractionDateTime(left, right);
        if (arguments[1] == WellKnownTypes.TimeSpan)
          return DateTimeCompilers.DateTimeOperatorSubtractionTimeSpan(left, right);
        throw new NotSupportedException();
      }
      if (firstArgument == WellKnownTypes.DateTimeOffset) {
        if (arguments[1] == WellKnownTypes.DateTime)
          return DateTimeOffsetCompilers.DateTimeOffsetOperatorSubtractionDateTimeOffset(left, right);
        if (arguments[1] == WellKnownTypes.TimeSpan)
          return DateTimeOffsetCompilers.DateTimeOffsetOperatorSubtractionTimeSpan(left, right);
        throw new NotSupportedException();
      }
      if (firstArgument == WellKnownTypes.TimeOnly)
        return TimeOnlyCompilers.TimeOnlyOperatorSubtraction(left, right);
      if (firstArgument == WellKnownTypes.Decimal)
        return DecimalCompilers.DecimalOperatorSubtraction(left, right);
      if (firstArgument == WellKnownTypes.TimeSpan)
        return TimeSpanCompilers.TimeSpanOperatorSubtraction(left, right);

      return SqlDml.Subtract(left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Multiply, TargetKind.Operator, 3)]
    public static SqlExpression OperationMultiply(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.Multiply(left, right);
    }

    [Compiler(FSharpLanguagePrimitives, "MultiplyDynamic", TargetKind.Static | TargetKind.Method, 3)]
    public static SqlExpression DynamicMultiply(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return OperationMultiply(member, left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Division, TargetKind.Operator, 3)]
    public static SqlExpression OperationDivision(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.Divide(left, right);
    }

    [Compiler(FSharpLanguagePrimitives, "DivisionDynamic", TargetKind.Static | TargetKind.Method, 3)]
    public static SqlExpression DynamicDivision(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return OperationDivision(member, left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Equality, TargetKind.Operator, 1)]
    public static SqlExpression OperationEquality(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.Equals(left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.Inequality, TargetKind.Operator, 1)]
    public static SqlExpression OperationInequality(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      return SqlDml.NotEquals(left, right);
    }

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.GreaterThan, TargetKind.Operator, 1)]
    public static SqlExpression OperationGreaterThan(MemberInfo member, SqlExpression left, SqlExpression right)
      => SqlDml.GreaterThan(left, right);

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.GreaterThanOrEqual, TargetKind.Operator, 1)]
    public static SqlExpression OperationGreaterThanOrEqual(MemberInfo member, SqlExpression left, SqlExpression right)
      => SqlDml.GreaterThanOrEquals(left, right);

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.LessThan, TargetKind.Operator, 1)]
    public static SqlExpression OperationLessThan(MemberInfo member, SqlExpression left, SqlExpression right)
      => SqlDml.LessThan(left, right);

    [Compiler(FSharpOperators, Reflection.WellKnown.Operator.LessThanOrEqual, TargetKind.Operator, 1)]
    public static SqlExpression OperationLessThanOrEqual(MemberInfo member, SqlExpression left, SqlExpression right)
      => SqlDml.LessThanOrEquals(left, right);

    [Compiler(FSharpOperators, "IsNull", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression OperationLessThanOrEqual(MemberInfo member, SqlExpression operand)
      => SqlDml.IsNull(operand);
  }
#pragma warning restore IDE0060 // Remove unused parameter
}