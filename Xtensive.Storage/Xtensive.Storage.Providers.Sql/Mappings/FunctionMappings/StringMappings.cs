// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.13

using System;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class StringMappings
  {
    private static readonly SqlLiteral<string> Percent = SqlFactory.Literal("%");
    
    [Compiler(typeof(string), "StartsWith")]
    public static SqlExpression StringStartsWith(SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(value, Percent));
    }
  
    [Compiler(typeof(string), "EndsWith")]
    public static SqlExpression StringEndsWith(SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(Percent, value));
    }

    [Compiler(typeof(string), "Contains")]
    public static SqlExpression StringContains(SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(this_, SqlFactory.Concat(Percent, SqlFactory.Concat(value, Percent)));
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression this_,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlFactory.Substring(this_, startIndex);
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression this_,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
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
      throw new NotImplementedException();
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression this_,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return TrimHelper(this_, trimChars, SqlTrimType.Both);
    }

    [Compiler(typeof(string), "TrimStart")]
    public static SqlExpression StringTrimStart(SqlExpression this_,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return TrimHelper(this_, trimChars, SqlTrimType.Leading);
    }

    [Compiler(typeof(string), "TrimEnd")]
    public static SqlExpression StringTrimEnd(SqlExpression this_,
      [Type(typeof(char[]))] SqlExpression trimChars)
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

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceCh(SqlExpression this_,
      [Type(typeof(char))] SqlExpression oldChar,
      [Type(typeof(char))] SqlExpression newChar)
    {
      return SqlFactory.Replace(this_, oldChar, newChar);
    }

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceStr(SqlExpression this_,
      [Type(typeof(string))] SqlExpression oldValue,
      [Type(typeof(string))] SqlExpression newValue)
    {
      return SqlFactory.Replace(this_, oldValue, newValue);
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression this_,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlFactory.Substring(this_, SqlFactory.Literal(0), startIndex);
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression this_,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression count)
    {
      return SqlFactory.Concat(
        SqlFactory.Substring(this_, SqlFactory.Literal(0), startIndex),
        SqlFactory.Substring(this_, startIndex + count));
    }

    [Compiler(typeof(string), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringIsNullOrEmpty(
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.IsNull(value) || SqlFactory.Length(value) == SqlFactory.Literal(0);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1)
    {
      return SqlFactory.Concat(str0, str1);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2)
    {
      return SqlFactory.Concat(SqlFactory.Concat(str0, str1), str2);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2,
      [Type(typeof(string))] SqlExpression str3)
    {
      return SqlFactory.Concat(SqlFactory.Concat(SqlFactory.Concat(str0, str1), str2), str3);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string[]))] SqlExpression values)
    {
      // before implementing ExpressionProcessor.VisitNewArray this does not matter
      throw new NotImplementedException();
    }

    [Compiler(typeof(string), "Compare", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringCompare(
      [Type(typeof(string))] SqlExpression strA,
      [Type(typeof(string))] SqlExpression strB)
    {
      var result = SqlFactory.Case();
      result.Add(strA > strB, SqlFactory.Literal(1));
      result.Add(strA < strB, SqlFactory.Literal(-1));
      result.Else = SqlFactory.Literal(0);
      return result;
    }

    [Compiler(typeof(string), "CompareTo")]
    public static SqlExpression StringCompareTo(SqlExpression this_,
      [Type(typeof(string))] SqlExpression strB)
    {
      return StringCompare(this_, strB);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfStr(SqlExpression this_,
      [Type(typeof(string))] SqlExpression str)
    {
      return SqlFactory.Position(str, this_);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfCh(SqlExpression this_,
      [Type(typeof(char))] SqlExpression ch)
    {
      return SqlFactory.Position(ch, this_);
    }

    [Compiler(typeof(string), "Equals")]
    public static SqlExpression StringEquals(SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return value is SqlNull ? (SqlExpression) SqlFactory.IsNull(this_) : this_==value;
    }

    [Compiler(typeof(StringExtensions), "LessThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThan(
      [Type(typeof(string))] SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.LessThan(this_, value);
    }

    [Compiler(typeof(StringExtensions), "LessThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThanOrEquals(
      [Type(typeof(string))] SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.LessThanOrEquals(this_, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThan(
      [Type(typeof(string))] SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.GreaterThan(this_, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThanOrEquals(
      [Type(typeof(string))] SqlExpression this_,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.GreaterThanOrEquals(this_, value);
    }

    [Compiler(typeof(string), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression StringOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlFactory.Equals(left, right);
    }

    [Compiler(typeof(string), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression StringOperatorInequality(SqlExpression left, SqlExpression right)
    {
      return SqlFactory.NotEquals(left, right);
    }
  }
}
