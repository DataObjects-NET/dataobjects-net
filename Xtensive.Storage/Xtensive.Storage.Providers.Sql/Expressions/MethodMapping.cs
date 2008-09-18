// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.10

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal static class MethodMapping
  {
    private static readonly ThreadSafeDictionary<MethodInfo, Func<SqlExpression, SqlExpression[], SqlExpression>> map =
      ThreadSafeDictionary<MethodInfo, Func<SqlExpression, SqlExpression[], SqlExpression>>.Create(new object());

    public static Func<SqlExpression, SqlExpression[], SqlExpression> GetMapping(MethodInfo methodInfo)
    {
      Func<SqlExpression, SqlExpression[], SqlExpression> value;
      if (!map.TryGetValue(methodInfo, out value))
        throw new NotSupportedException();
      return value;
    }

    static MethodMapping()
    {
      var type = typeof(string);
      var percent = SqlFactory.Literal("%");
      var method = type.GetMethod("EndsWith", new[]{typeof(string)});
      map.SetValue(method, (target, arguments) => SqlFactory.Like(target, SqlFactory.Concat(percent, arguments[0])));
      method = type.GetMethod("StartsWith", new[] { typeof(string) });
      map.SetValue(method, (target, arguments) => SqlFactory.Like(target, SqlFactory.Concat(arguments[0], percent)));
      method = type.GetMethod("Contains", new[] { typeof(string) });
      map.SetValue(method, (target, arguments) => SqlFactory.Like(target, SqlFactory.Concat(percent ,SqlFactory.Concat(arguments[0], percent))));
      method = type.GetMethod("Substring", new[] { typeof(int) });
      map.SetValue(method, (target, arguments) => SqlFactory.Substring(target, arguments[0]));
      method = type.GetMethod("Substring", new[] { typeof(int), typeof(int)});
      map.SetValue(method, (target, arguments) => SqlFactory.Substring(target, arguments[0], arguments[1]));
      method = type.GetMethod("ToLower", ArrayUtils<Type>.EmptyArray);
      map.SetValue(method, (target, _) => SqlFactory.Lower(target));
      method = type.GetMethod("ToUpper", ArrayUtils<Type>.EmptyArray);
      map.SetValue(method, (target, _) => SqlFactory.Upper(target));
      method = type.GetMethod("Trim", ArrayUtils<Type>.EmptyArray);
      map.SetValue(method, (target, _) => SqlFactory.Trim(target));
      method = type.GetMethod("Trim", new[]{typeof(char[])});
      map.SetValue(method, (target, arguments) => SqlFactory.Trim(target, SqlTrimType.Both, ((SqlLiteral<char[]>)arguments[0]).Value[0]));
      method = type.GetMethod("TrimEnd", new[] { typeof(char[]) });
      map.SetValue(method, (target, arguments) => SqlFactory.Trim(target, SqlTrimType.Trailing, ((SqlLiteral<char[]>)arguments[0]).Value[0]));
      method = type.GetMethod("TrimStart", new[] { typeof(char[]) });
      map.SetValue(method, (target, arguments) => SqlFactory.Trim(target, SqlTrimType.Leading, ((SqlLiteral<char[]>)arguments[0]).Value[0]));
    }
  }
}