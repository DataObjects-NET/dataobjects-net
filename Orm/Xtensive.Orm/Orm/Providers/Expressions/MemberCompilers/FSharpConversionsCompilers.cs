// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Reflection;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
#pragma warning disable IDE0060 // Remove unused parameter
  [CompilerContainer(typeof(SqlExpression))]
  internal static class FSharpConversionsCompilers
  {
#if NET8_0_OR_GREATER
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif NET7_0
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
    private const string FSharpOperators = "Microsoft.FSharp.Core.Operators, FSharp.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#endif

    [Compiler(FSharpOperators, "ToChar", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToCharGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToChar(stringExpression);
    }

    [Compiler(FSharpOperators, "ToByte", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToByteGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToByte(stringExpression);
    }

    [Compiler(FSharpOperators, "ToSByte", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToSByteGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToSbyte(stringExpression);
    }

    [Compiler(FSharpOperators, "ToInt16", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToShortGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToShort(stringExpression);
    }

    [Compiler(FSharpOperators, "ToUInt16", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToUShortGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUshort(stringExpression);
    }

    [Compiler(FSharpOperators, "ToInt", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToIntGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToInt(stringExpression);
    }

    [Compiler(FSharpOperators, "ToInt32", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToInt32Generic(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToInt(stringExpression);
    }

    [Compiler(FSharpOperators, "ToUInt32", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToUInt32Generic(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUint(stringExpression);
    }

    [Compiler(FSharpOperators, "ToInt64", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToInt64Generic(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToLong(stringExpression);
    }

    [Compiler(FSharpOperators, "ToUInt64", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToUInt64Generic(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUlong(stringExpression);
    }

    [Compiler(FSharpOperators, "ToSingle", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToSingleGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToFloat(stringExpression);
    }

    [Compiler(FSharpOperators, "ToDouble", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToDoubleGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDouble(stringExpression);
    }

    [Compiler(FSharpOperators, "ToDecimal", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToDecimalGeneric(MemberInfo memberInfo, SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDecimal(stringExpression);
    }

    [Compiler(FSharpOperators, "ToString", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression ToStringGeneric(MemberInfo memberInfo, SqlExpression expression)

    {
      return ExpressionTranslationHelpers.ToString(expression);
    }
  }
#pragma warning restore IDE0060 // Remove unused parameter
}