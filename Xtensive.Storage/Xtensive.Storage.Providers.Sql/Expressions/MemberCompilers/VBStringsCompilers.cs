// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using Xtensive.Core.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VbStringsCompilers
  {
    #if NET40
      private const string VbStrings = "Microsoft.VisualBasic.Strings, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbStrings = "Microsoft.VisualBasic.Strings, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #endif

    [Compiler(VbStrings, "Trim", TargetKind.Static)]
    public static SqlExpression Trim(SqlExpression stringExpression)
    {
      return SqlDml.Trim(stringExpression);
    }

    [Compiler(VbStrings, "LTrim", TargetKind.Static)]
    public static SqlExpression LTrim(SqlExpression stringExpression)
    {
      return SqlDml.Trim(stringExpression, SqlTrimType.Leading);
    }

    [Compiler(VbStrings, "RTrim", TargetKind.Static)]
    public static SqlExpression RTrim(SqlExpression stringExpression)
    {
      return SqlDml.Trim(stringExpression, SqlTrimType.Trailing);
    }

    [Compiler(VbStrings, "Len", TargetKind.Static)]
    public static SqlExpression Len([Type(typeof(string))] SqlExpression stringExpression)
    {
      return SqlDml.CharLength(stringExpression);
    }

    [Compiler(VbStrings, "Left", TargetKind.Static)]
    public static SqlExpression Left(SqlExpression stringExpression, SqlExpression lengthExpression)
    {
      return SqlDml.Substring(stringExpression, SqlDml.Literal(0), lengthExpression);
    }

    [Compiler(VbStrings, "Right", TargetKind.Static)]
    public static SqlExpression Rigth(SqlExpression stringExpression, SqlExpression lengthExpression)
    {
      return SqlDml.Substring(stringExpression, Len(stringExpression) - lengthExpression, lengthExpression);
    }

    [Compiler(VbStrings, "Mid", TargetKind.Static)]
    public static SqlExpression Mid2(SqlExpression stringExpression, SqlExpression startExpression)
    {
      return SqlDml.Substring(stringExpression, startExpression);
    }

    [Compiler(VbStrings, "Mid", TargetKind.Static)]
    public static SqlExpression Mid3(SqlExpression stringExpression, SqlExpression startExpression, SqlExpression lengthExpression)
    {
      return SqlDml.Substring(stringExpression, startExpression, lengthExpression);
    }

    [Compiler(VbStrings, "UCase", TargetKind.Static)]
    public static SqlExpression UCaseString([Type(typeof(string))]SqlExpression stringExpression)
    {
      return SqlDml.Upper(stringExpression);
    }

    [Compiler(VbStrings, "UCase", TargetKind.Static)]
    public static SqlExpression UCaseChar([Type(typeof(char))]SqlExpression charExpression)
    {
      return SqlDml.Upper(charExpression);
    }

    [Compiler(VbStrings, "LCase", TargetKind.Static)]
    public static SqlExpression LCaseString([Type(typeof(string))]SqlExpression stringExpression)
    {
      return SqlDml.Lower(stringExpression);
    }

    [Compiler(VbStrings, "LCase", TargetKind.Static)]
    public static SqlExpression LCaseChar([Type(typeof(char))]SqlExpression charExpression)
    {
      return SqlDml.Lower(charExpression);
    }
  }
}
