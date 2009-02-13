// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.13

using System.Linq;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class StringMappings
  {
    private static readonly SqlLiteral<string> Percent = SqlFactory.Literal("%");
    
    [Compiler(typeof(string), "StartsWith")]
    public static SqlExpression StringStartsWith(SqlExpression this_,
      [ParamType(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(value, Percent));
    }
  
    [Compiler(typeof(string), "EndsWith")]
    public static SqlExpression StringEndsWith(SqlExpression this_,
      [ParamType(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(Percent, value));
    }

    [Compiler(typeof(string), "Contains")]
    public static SqlExpression StringContains(SqlExpression this_,
      [ParamType(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(Percent, SqlFactory.Concat(value, Percent)));
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression this_,
      [ParamType(typeof(int))] SqlExpression startIndex)
    {
      return SqlFactory.Substring(this_, startIndex);
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression this_,
      [ParamType(typeof(int))] SqlExpression startIndex,
      [ParamType(typeof(int))] SqlExpression length)
    {
      return SqlFactory.Substring(this_, startIndex, length);
    }

    [Compiler(typeof(string), "ToUpper")]
    public static SqlExpression StringToUpper(SqlExpression this_)
    {
      return SqlFactory.Upper(this_);
    }

    [Compiler(typeof(string), "ToLower")]
    public static SqlExpression StringToLower(SqlExpression this_)
    {
      return SqlFactory.Lower(this_);
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression this_)
    {
      return SqlFactory.Trim(this_);
    }

    private static SqlExpression TrimHelper(SqlExpression this_,
      SqlExpression trimChars, SqlTrimType trimType)
    {
      var chars = trimChars as SqlLiteral<char[]>;
      if (chars == null)
        return SqlFactory.Trim(this_, trimType, ' ');

      var exp = this_;
      foreach (var ch in chars.Value.Distinct())
        exp = SqlFactory.Trim(exp, trimType, ch);

      return exp;
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression this_,
      [ParamType(typeof(char[]))] SqlExpression trimChars)
    {
      return TrimHelper(this_, trimChars, SqlTrimType.Both);
    }

    [Compiler(typeof(string), "TrimStart")]
    public static SqlExpression StringTrimStart(SqlExpression this_,
      [ParamType(typeof(char[]))] SqlExpression trimChars)
    {
      return TrimHelper(this_, trimChars, SqlTrimType.Leading);
    }

    [Compiler(typeof(string), "TrimEnd")]
    public static SqlExpression StringTrimEnd(SqlExpression this_,
      [ParamType(typeof(char[]))] SqlExpression trimChars)
    {
      return TrimHelper(this_, trimChars, SqlTrimType.Trailing);
    }

    [Compiler(typeof(string), "Length", TargetKind.PropertyGet)]
    public static SqlExpression StringLength(SqlExpression this_)
    {
      return SqlFactory.Length(this_);
    }

    [Compiler(typeof(string), "ToString")]
    public static SqlExpression StringToString(SqlExpression this_)
    {
      return this_;
    }

    [Compiler(typeof(string), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringIsNullOrEmpty(
      [ParamType(typeof(string))] SqlExpression value)
    {
      return SqlFactory.IsNull(value) || SqlFactory.Length(value) == SqlFactory.Literal(0);
    }
  }
}
