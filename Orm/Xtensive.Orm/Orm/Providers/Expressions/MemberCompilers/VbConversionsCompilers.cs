// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VbConversionsCompilers
  {
    #if NET40
      private const string VbConversions = "Microsoft.VisualBasic.CompilerServices.Conversions, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbConversions = "Microsoft.VisualBasic.CompilerServices.Conversions, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #endif

    [Compiler(VbConversions, "ToBoolean", TargetKind.Static)]
    public static SqlExpression ToBoolean([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDecimal(stringExpression);
    }

    [Compiler(VbConversions, "ToByte", TargetKind.Static)]
    public static SqlExpression ToByte([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToByte(stringExpression);
    }

    [Compiler(VbConversions, "ToChar", TargetKind.Static)]
    public static SqlExpression ToChar([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToChar(stringExpression);
    }

    [Compiler(VbConversions, "ToDate", TargetKind.Static)]
    public static SqlExpression ToDate([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDateTime(stringExpression);
    }

    [Compiler(VbConversions, "ToDecimal", TargetKind.Static)]
    public static SqlExpression ToDecimalFromString([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDecimal(stringExpression);
    }

    [Compiler(VbConversions, "ToDecimal", TargetKind.Static)]
    public static SqlExpression ToDecimalFromBoolean([Type(typeof(bool))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDecimal(stringExpression);
    }

    [Compiler(VbConversions, "ToDouble", TargetKind.Static)]
    public static SqlExpression ToDouble([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToDouble(stringExpression);
    }

    [Compiler(VbConversions, "ToInteger", TargetKind.Static)]
    public static SqlExpression ToInteger([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToInt(stringExpression);
    }

    [Compiler(VbConversions, "ToLong", TargetKind.Static)]
    public static SqlExpression ToLong([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToLong(stringExpression);
    }

    [Compiler(VbConversions, "ToSByte", TargetKind.Static)]
    public static SqlExpression ToSByte([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToSbyte(stringExpression);
    }

    [Compiler(VbConversions, "ToShort", TargetKind.Static)]
    public static SqlExpression ToShort([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToShort(stringExpression);
    }

    [Compiler(VbConversions, "ToSingle", TargetKind.Static)]
    public static SqlExpression ToSingle([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToFloat(stringExpression);
    }

    [Compiler(VbConversions, "ToUInteger", TargetKind.Static)]
    public static SqlExpression ToUInteger([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUint(stringExpression);
    }

    [Compiler(VbConversions, "ToULong", TargetKind.Static)]
    public static SqlExpression ToULong([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUlong(stringExpression);
    }

    [Compiler(VbConversions, "ToUShort", TargetKind.Static)]
    public static SqlExpression ToUShort([Type(typeof(string))]SqlExpression stringExpression)
    {
      return ExpressionTranslationHelpers.ToUshort(stringExpression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromBoolean([Type(typeof(bool))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromByte([Type(typeof(byte))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromDecimal([Type(typeof(decimal))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromDate([Type(typeof(DateTime))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromChar([Type(typeof(char))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromDouble([Type(typeof(double))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromShort([Type(typeof(short))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromLong([Type(typeof(long))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromSingle([Type(typeof(float))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromUInt32([Type(typeof(uint))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromUInt64([Type(typeof(ulong))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }

    [Compiler(VbConversions, "ToString", TargetKind.Static)]
    public static SqlExpression ToStringFromInteger([Type(typeof(int))]SqlExpression expression)
    {
      return ExpressionTranslationHelpers.ToString(expression);
    }
  }
}
