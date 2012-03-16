// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.13

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Linq;
using Xtensive.Orm;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class StringCompilers
  {
    private static SqlExpression GenericLike(SqlExpression _this,
      SqlExpression patternExpression, bool percentAtStart, bool percentAtEnd)
    {
      const string percent = "%";
      const string ground = "_";
      const string escape = "^";
      const string escapeEscape = "^^";
      const string escapeGround = "^_";
      const string escapePercent = "^%";

      var stringPattern = patternExpression as SqlLiteral<string>;
      var charPattern = patternExpression as SqlLiteral<char>;
      if (ReferenceEquals(stringPattern, null) && ReferenceEquals(charPattern, null)) {
        SqlExpression result = SqlDml.Replace(patternExpression, SqlDml.Literal(escape), SqlDml.Literal(escapeEscape));
        result = SqlDml.Replace(result, SqlDml.Literal(ground), SqlDml.Literal(escapeGround));
        result = SqlDml.Replace(result, SqlDml.Literal(percent), SqlDml.Literal(escapePercent));
        if (percentAtStart)
          result = SqlDml.Concat(SqlDml.Literal(percent), result);
        if (percentAtEnd)
          result = SqlDml.Concat(result, SqlDml.Literal(percent));

        result = SqlDml.Like(_this, result, escape);
        return result;
      }
      var originalPattern = !ReferenceEquals(stringPattern, null)
        ? stringPattern.Value
        : charPattern.Value.ToString();
      var escapedPattern = new StringBuilder(originalPattern);
      escapedPattern
        .Replace(escape, escapeEscape)
        .Replace(percent, escapePercent)
        .Replace(ground, escapeGround);
      bool escaped = escapedPattern.Length > originalPattern.Length;
      if (percentAtStart)
        escapedPattern.Insert(0, percent);
      if (percentAtEnd)
        escapedPattern.Append(percent);
      var pattern = escapedPattern.ToString();
      return escaped
        ? SqlDml.Like(_this, pattern, escape)
        : SqlDml.Like(_this, pattern);
    }

    [Compiler(typeof(string), "StartsWith")]
    public static SqlExpression StringStartsWith(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return GenericLike(_this, value, false, true);
    }
  
    [Compiler(typeof(string), "EndsWith")]
    public static SqlExpression StringEndsWith(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return GenericLike(_this, value, true, false);
    }

    [Compiler(typeof(string), "Contains")]
    public static SqlExpression StringContains(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return GenericLike(_this, value, true, true);
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlDml.Substring(_this, startIndex);
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return SqlDml.Substring(_this, startIndex, length);
    }

    [Compiler(typeof(string), "ToUpper")]
    public static SqlExpression StringToUpper(SqlExpression _this)
    {
      return SqlDml.Upper(_this);
    }

    [Compiler(typeof(string), "ToLower")]
    public static SqlExpression StringToLower(SqlExpression _this)
    {
      return SqlDml.Lower(_this);
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression _this)
    {
      return SqlDml.Trim(_this);
    }

    private static SqlExpression GenericTrim(SqlExpression _this, SqlExpression trimChars, SqlTrimType trimType)
    {
      if (trimChars is SqlNull)
        return SqlDml.Trim(_this, trimType);
      var container = trimChars as SqlContainer;
      if (container.IsNullReference())
        throw new NotSupportedException(Strings.ExStringTrimSupportedOnlyWithConstants);
      var chars = container.Value as char[];
      if (chars==null)
        throw new NotSupportedException(Strings.ExStringTrimSupportedOnlyWithConstants);
      return chars.Length==0
        ? SqlDml.Trim(_this, trimType)
        : SqlDml.Trim(_this, trimType, new string(chars));
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Both);
    }

    [Compiler(typeof(string), "TrimStart")]
    public static SqlExpression StringTrimStart(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Leading);
    }

    [Compiler(typeof(string), "TrimEnd")]
    public static SqlExpression StringTrimEnd(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Trailing);
    }

    [Compiler(typeof(string), "Length", TargetKind.PropertyGet)]
    public static SqlExpression StringLength(SqlExpression _this)
    {
      return SqlDml.CharLength(_this);
    }

    [Compiler(typeof(string), "ToString")]
    public static SqlExpression StringToString(SqlExpression _this)
    {
      return _this;
    }

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceCh(SqlExpression _this,
      [Type(typeof(char))] SqlExpression oldChar,
      [Type(typeof(char))] SqlExpression newChar)
    {
      return SqlDml.Replace(_this, oldChar, newChar);
    }

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceStr(SqlExpression _this,
      [Type(typeof(string))] SqlExpression oldValue,
      [Type(typeof(string))] SqlExpression newValue)
    {
      return SqlDml.Replace(_this, oldValue, newValue);
    }

    [Compiler(typeof(string), "Insert")]
    public static SqlExpression StringInsert(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.Concat(SqlDml.Concat(
        SqlDml.Substring(_this, 0, startIndex), value),
        SqlDml.Substring(_this, startIndex, SqlDml.CharLength(_this) - startIndex));
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlDml.Substring(_this, SqlDml.Literal(0), startIndex);
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression count)
    {
      return SqlDml.Concat(
        SqlDml.Substring(_this, SqlDml.Literal(0), startIndex),
        SqlDml.Substring(_this, startIndex + count));
    }

    [Compiler(typeof(string), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringIsNullOrEmpty(
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.IsNull(value) || SqlDml.CharLength(value)==SqlDml.Literal(0);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1)
    {
      return SqlDml.Concat(str0, str1);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2)
    {
      return SqlDml.Concat(SqlDml.Concat(str0, str1), str2);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2,
      [Type(typeof(string))] SqlExpression str3)
    {
      return SqlDml.Concat(SqlDml.Concat(SqlDml.Concat(str0, str1), str2), str3);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string[]))] SqlExpression values)
    {
      if (values.NodeType!=SqlNodeType.Container)
        throw new NotSupportedException();
      var container = (SqlContainer) values;
      if (container.Value.GetType() != typeof(SqlExpression[]))
        throw new NotSupportedException();
      var expressions = (SqlExpression[]) container.Value;
      if (expressions.Length==0)
        return SqlDml.Literal("");
      return expressions.Aggregate(SqlDml.Concat);
    }

    [Compiler(typeof(string), "PadLeft")]
    public static SqlExpression StringPadLeft(SqlExpression _this,
      [Type(typeof(int))] SqlExpression totalWidth)
    {
      return SqlDml.PadLeft(_this, totalWidth);
    }
    
    [Compiler(typeof(string), "PadLeft")]
    public static SqlExpression StringPadLeft(SqlExpression _this,
      [Type(typeof(int))] SqlExpression totalWidth,
      [Type(typeof(char))] SqlExpression paddingChar)
    {
      return SqlDml.PadLeft(_this, totalWidth, paddingChar);
    }

    [Compiler(typeof(string), "PadRight")]
    public static SqlExpression StringPadRight(SqlExpression _this,
      [Type(typeof(int))] SqlExpression totalWidth)
    {
      return SqlDml.PadRight(_this, totalWidth);
    }

    [Compiler(typeof(string), "PadRight")]
    public static SqlExpression StringPadRight(SqlExpression _this,
      [Type(typeof(int))] SqlExpression totalWidth,
      [Type(typeof(char))] SqlExpression paddingChar)
    {
      return SqlDml.PadRight(_this, totalWidth, paddingChar);
    }

    [Compiler(typeof(string), "Compare", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringCompare(
      [Type(typeof(string))] SqlExpression strA,
      [Type(typeof(string))] SqlExpression strB)
    {
      var result = SqlDml.Case();
      result.Add(strA > strB, SqlDml.Literal(1));
      result.Add(strA < strB, SqlDml.Literal(-1));
      result.Else = SqlDml.Literal(0);
      return result;
    }

    [Compiler(typeof(string), "CompareTo")]
    public static SqlExpression StringCompareTo(SqlExpression _this,
      [Type(typeof(string))] SqlExpression strB)
    {
      return StringCompare(_this, strB);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring)
    {
      return SqlDml.Position(substring, _this);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring, SqlExpression startIndex)
    {
      return SqlDml.Coalesce(startIndex + 
        SqlDml.NullIf(SqlDml.Position(substring, SqlDml.Substring(_this, startIndex)), -1),
        -1);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring, SqlExpression startIndex, SqlExpression length)
    {
      return SqlDml.Coalesce(startIndex + 
        SqlDml.NullIf(SqlDml.Position(substring, SqlDml.Substring(_this, startIndex, length)), -1),
        -1);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str)
    {
      return GenericStringIndexOf(_this, str);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return GenericStringIndexOf(_this, str, startIndex);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return GenericStringIndexOf(_this, str, startIndex, length);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch)
    {
      return GenericStringIndexOf(_this, ch);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return GenericStringIndexOf(_this, ch, startIndex);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return GenericStringIndexOf(_this, ch, startIndex, length);
    }

    [Compiler(typeof(string), "Chars", TargetKind.PropertyGet)]
    public static SqlExpression StringChars(SqlExpression _this, [Type(typeof(int))] SqlExpression index)
    {
      return SqlDml.Substring(_this, index, 1);
    }

    [Compiler(typeof(string), "Equals")]
    public static SqlExpression StringEquals(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return value is SqlNull ? (SqlExpression) SqlDml.IsNull(_this) : _this==value;
    }

    [Compiler(typeof(StringExtensions), "LessThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThan(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.LessThan(_this, value);
    }

    [Compiler(typeof(StringExtensions), "LessThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThanOrEquals(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.LessThanOrEquals(_this, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThan(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.GreaterThan(_this, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThanOrEquals(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlDml.GreaterThanOrEquals(_this, value);
    }

    [Compiler(typeof(EnumerableExtensions), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression EnumerableIsNullOrEmptyExtension(
      MemberInfo member, SqlExpression value)
    {
      var method = (MethodInfo) member;
      if (method.GetGenericArguments()[0] != typeof(string))
        throw new NotSupportedException();
      return StringIsNullOrEmpty(value);
    }

    [Compiler(typeof(Enumerable), "Contains", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression EnumerableContains(
      MemberInfo member, SqlExpression sequence, SqlExpression value)
    {
      var method = (MethodInfo) member;
      // Try string.Contains first
      if (method.GetGenericArguments()[0]==typeof (char))
        return StringContains(sequence, value);
      // Otherwise translate into general IN clause
      var container = sequence as SqlContainer;
      if (container.IsNullReference())
        throw new NotSupportedException(Strings.ExTranslationOfInContainsIsNotSupportedInThisCase);
      var items = container.Value as IEnumerable;
      if (items == null)
        throw new NotSupportedException(Strings.ExTranslationOfInContainsIsNotSupportedInThisCase);
      var expressions = new List<SqlExpression>();
      foreach (var item in items)
        expressions.Add(SqlDml.Literal(item));
      return SqlDml.In(value, SqlDml.Row(expressions));
    }

    [Compiler(typeof (QueryableExtensions), "In", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression EnumerableExtensionsInEnumerable(
      MemberInfo member, SqlExpression value, [Type(typeof(IEnumerable<>))] SqlExpression sequence)
    {
      return EnumerableContains(member, sequence, value);
    }

    [Compiler(typeof(QueryableExtensions), "In", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression EnumerableExtensionsInArray(
      MemberInfo member, SqlExpression value, [Type(typeof(MethodHelper.AnyArrayPlaceholder))] SqlExpression sequence)
    {
      return EnumerableContains(member, sequence, value);
    }


    [Compiler(typeof(string), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression StringOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.Equals(left, right);
    }

    [Compiler(typeof(string), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression StringOperatorInequality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.NotEquals(left, right);
    }
  }
}