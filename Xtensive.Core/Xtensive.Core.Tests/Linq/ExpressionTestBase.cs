// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Xtensive.Core.Tests.Linq
{
  [Serializable]
  public class ExpressionTestBase
  {
    private LambdaExpression[] expressions;

    public IEnumerable<LambdaExpression> Expressions { get { return expressions; } }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      expressions = new LambdaExpression[]
        {
          (Expression<Func<int, int>>) (k => k + 1),
          (Expression<Func<object, object>>) (p => p.ToString()),
          (Expression<Action<int, int>>) ((a, b) => Console.Write("{0} + {1} = {2}", a, b, a + b)),
          (Expression<Func<string, object>>) (s => new {Value = s}),
          (Expression<Func<DateTime, string>>) (d => (d - DateTime.Now).Duration().ToString()),
          (Expression<Func<long, Expression<Func<long>>>>) (x => Expression.Lambda<Func<long>>(Expression.Constant(x))),
          (Expression<Func<int, int, int, DateTime>>) ((y, m, d) => new DateTime(y, m, d))
        };
    }
  }
}