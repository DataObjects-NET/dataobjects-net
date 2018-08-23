// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [Serializable]
  public class ExpressionTestBase
  {
    public LambdaExpression[] Expressions { get; private set; }

    #region Nested types : Helper class, struct

    private struct Struct
    {
      public int Id;
    }

    private class Helper
    {
      public int Field;

      public Helper NestedClass;

      public List<int> NestedCollection;

      public int InstanceGenericMethod<T>(int value)
      {
        return value + typeof(T).Name.Length + +GetHashCode();
      }

      public static int StaticGenericMethod<T>(int value)
      {
        return value + typeof (T).GetHashCode();
      }
    }

    #endregion

    [OneTimeSetUp]
    public virtual void TestFixtureSetUp()
    {
      Expressions = new LambdaExpression[]
        {
          // Simple expression
          (Expression<Func<int, int>>) (k => k + 1),

          // Instance method call
          (Expression<Func<object, object>>) (p => p.ToString()),

          // Static method call
          (Expression<Action<int, int>>) ((a, b) => Console.Write("{0} + {1} = {2}", a, b, a + b)),

          // Instance generic method call
          (Expression<Func<Helper, int>>) (h => h.InstanceGenericMethod<long>(0)),

          // Static generic method call
          (Expression<Func<int>>) (() => Helper.StaticGenericMethod<long>(0)),

          // Instance generic method call (with generic argument being generic type)
          (Expression<Func<Helper, int>>) (h => h.InstanceGenericMethod<Func<int>>(0)),

          // Static generic method call (with generic argument being generic type)
          (Expression<Func<int>>) (() => Helper.StaticGenericMethod<Func<int>>(0)),

          // Static (extension) generic method call
          (Expression<Func<IEnumerable<Func<int>>, IEnumerable<int>>>) (funcs => funcs.Select(f => f.Invoke())),

          // Anonymous type constructor
          (Expression<Func<string, object>>) (s => new {Value = s}),

          // Static property access + binary operator
          (Expression<Func<DateTime, string>>) (d => (d - DateTime.Now).Duration().ToString()),

          // Constructor + a lots of generics
          (Expression<Func<int, List<int>>>) (i => new List<int>(i)),
          (Expression<Func<int, List<List<int>>>>) (i => new List<List<int>>(i)),
          (Expression<Func<int, List<List<List<int>>>>>) (i => new List<List<List<int>>>(i)),
          (Expression<Func<int, List<List<List<List<int>>>>>>) (i => new List<List<List<List<int>>>>(i)),

          // Static generic method call + static method call + constant expression
          (Expression<Func<long, Expression<Func<long>>>>) (x => Expression.Lambda<Func<long>>(Expression.Constant(x))),

          // Constructor
          (Expression<Func<int, int, int, DateTime>>) ((y, m, d) => new DateTime(y, m, d)),

          // List init expression
          (Expression<Func<string, List<int>>>) (s => new List<int> {s.Length}),

          // Array init expression
          (Expression<Func<Guid, Guid[]>>) (g => new[] {g}),

          // Delegate call
          (Expression<Func<int>>) (() => ((Func<int, int>) (x => x + 1)).Invoke(5)),

          // Conditional + new array + static property of generic type
          // Stupid casts are required because otherwise expression won't construct at runtime
          (Expression<Func<int, byte[]>>) (k => k <= 0 ? (byte[]) ArrayUtils<byte>.EmptyArray : (byte[]) new byte[k]),

          // TypeIs
          (Expression<Func<object, bool>>) (o => o is string),

          // Coalesce + constructor + member assignment
          (Expression<Func<object, object>>) (o => o ?? new Helper {Field = 5}),

          // Member list init
          (Expression<Func<object>>) (() => new Helper {NestedCollection = {1,2,3}}),

          // Member member binding
          (Expression<Func<object>>) (() => new Helper {NestedClass = {Field = 5}}),

          // Struct constructor
          (Expression<Func<int, Struct>>) (a => new Struct {Id = 2}),

          // Don't know how to write InvocationExpression in C# syntax :-(
          Expression.Lambda<Func<int>>(
            Expression.Invoke(
              Expression.Lambda<Func<int, int>>(Expression.Constant(0), Expression.Parameter(typeof(int), "p")),
              Expression.Constant(1)))
        };
    }
  }
}