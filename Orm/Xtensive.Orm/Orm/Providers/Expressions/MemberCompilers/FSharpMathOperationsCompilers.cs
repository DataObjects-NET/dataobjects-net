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
  internal static class FSharpMathOperationsCompilers
  {
#if NET8_0_OR_GREATER
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif NET7_0
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#endif

    [Compiler(FSharpOperators, "Abs", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression AbsGeneric(MemberInfo member, SqlExpression operand)
    {
      return SqlDml.Abs(operand);
    }

    [Compiler(FSharpOperators, "Sign", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression SignGeneric(MemberInfo member, SqlExpression operand)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Sign(operand));
    }

    [Compiler(FSharpOperators, "Acos", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression AcosGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Acos(x);
    }

    [Compiler(FSharpOperators, "Asin", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression AsinGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Asin(x);
    }

    [Compiler(FSharpOperators, "Atan", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression AtanGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Atan(x);
    }

    [Compiler(FSharpOperators, "Atan2", TargetKind.Static | TargetKind.Method, 2)]
    public static SqlExpression Atan2Generic(MemberInfo member, SqlExpression x, SqlExpression y)
    {
      return SqlDml.Atan2(x, y);
    }

    [Compiler(FSharpOperators, "Ceiling", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression CeilingGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Ceiling(x);
    }

    [Compiler(FSharpOperators, "Cos", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression CosGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Cos(x);
    }

    [Compiler(FSharpOperators, "Cosh", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression CoshGeneric(MemberInfo member, SqlExpression x)
    {
      return (SqlDml.Exp(x) + SqlDml.Exp(-x)) / SqlDml.Literal(2);
    }

    [Compiler(FSharpOperators, "Exp", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ExpGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Exp(x);
    }

    [Compiler(FSharpOperators, "Floor", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression FloorGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Floor(x);
    }

    [Compiler(FSharpOperators, "Log", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression LogGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Log(x);
    }

    [Compiler(FSharpOperators, "Log10", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression Log10Generic(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Log10(x);
    }

    [Compiler(FSharpOperators, "Max", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression MaxGeneric(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      var result = SqlDml.Case();
      _ = result.Add(left > right, left);
      result.Else = right;
      return result;
    }

    [Compiler(FSharpOperators, "Min", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression MinGeneric(MemberInfo member, SqlExpression left, SqlExpression right)
    {
      var result = SqlDml.Case();
      _ = result.Add(left < right, left);
      result.Else = right;
      return result;
    }

    [Compiler(FSharpOperators, "Round", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression RoundGeneric(MemberInfo member, SqlExpression x)
    {
      return BankersRound(x, null, (member as MethodInfo).GetGenericArguments()[0] == WellKnownTypes.Decimal);
    }

    private static SqlExpression BankersRound(SqlExpression value, SqlExpression digits, bool isDecimal)
    {
      if (!isDecimal) {
        return SqlDml.Round(value, digits, TypeCode.Double, MidpointRounding.ToEven);
      }
      if (digits == null) {
        return TryCastToDecimalPS(SqlDml.Round(value, digits, TypeCode.Decimal, MidpointRounding.ToEven), 28, 0);
      }
      if (!(digits is SqlLiteral<int> scale)) {
        throw new NotSupportedException();
      }
      return TryCastToDecimalPS(SqlDml.Round(value, digits, TypeCode.Decimal, MidpointRounding.ToEven), 28, Convert.ToInt16(scale.Value));
    }

    private static SqlExpression TryCastToDecimalPS(SqlExpression value, short precision, short scale)
    {
      var context = ExpressionTranslationContext.Current;
      var provider = context.ProviderInfo.ProviderName;
      if (provider.Equals(WellKnown.Provider.PostgreSql, StringComparison.Ordinal)
        || provider.Equals(WellKnown.Provider.Oracle, StringComparison.Ordinal)) {
        // to fit result into .Net decimal since Npgsql client libarary does not provide a way to make in on reading
        return SqlDml.Cast(value, SqlType.Decimal, precision, scale);
      }
      return value;
    }


    [Compiler(FSharpOperators, "Sin", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression SinGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Sin(x);
    }

    [Compiler(FSharpOperators, "Sinh", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression SinhGeneric(MemberInfo member, SqlExpression x)
    {
      return (SqlDml.Exp(x) - SqlDml.Exp(-x)) / SqlDml.Literal(2);
    }

    [Compiler(FSharpOperators, "Sqrt", TargetKind.Static | TargetKind.Method, 2)]
    public static SqlExpression SqrtGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Sqrt(x);
    }

    [Compiler(FSharpOperators, "Tan", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression TanGeneric(MemberInfo member, SqlExpression x)
    {
      return SqlDml.Tan(x);
    }

    [Compiler(FSharpOperators, "Tanh", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression TanhGeneric(MemberInfo member, SqlExpression x)
    {
      var exp2d = SqlDml.Exp(SqlDml.Literal(2) * x);
      var one = SqlDml.Literal(1);

      return (exp2d - one) / (exp2d + one);
    }

    [Compiler(FSharpOperators, "Truncate", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression TruncateGeneric(MemberInfo member, SqlExpression x)
    {
      if ((member as MethodInfo).GetGenericArguments()[0] == WellKnownTypes.Decimal)
        return TryCastToDecimalPS(SqlDml.Truncate(x), 28, 0);
      return SqlDml.Truncate(x);
    }
  }
#pragma warning restore IDE0060 // Remove unused parameter
}
