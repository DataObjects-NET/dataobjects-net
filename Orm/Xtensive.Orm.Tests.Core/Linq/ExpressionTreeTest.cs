// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [TestFixture]
  public class ExpressionTreeTest : ExpressionTestBase
  {
    [Test]
    public void ComplexTest()
    {
      foreach (var e in Expressions) {
        Console.WriteLine(e.ToString(true));
        var x = e.ToExpressionTree();
        var y = e.ToExpressionTree();
        Assert.AreEqual(x, y);
        Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void SameExpressionTest()
    {
      var e = CreateSum().ToExpressionTree();
      Assert.AreEqual(e, e);
      Assert.AreEqual(e.GetHashCode(), e.GetHashCode());
    }
    
    [Test]
    public void SameDeclarationTest()
    {
      var x = CreateSum().ToExpressionTree();
      var y = CreateSum().ToExpressionTree();
      Assert.AreEqual(x, y);
      Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
    }

    [Test]
    public void NotEqualTest()
    {
      var x = CreateSum().ToExpressionTree();
      var y = CreateProduct().ToExpressionTree();
      Assert.AreNotEqual(x, y);
    }

    [Test]
    public void DifferentParameterNamesTest()
    {
      var x = CreateSum().ToExpressionTree();
      var y = CreateSum2().ToExpressionTree();
      Assert.AreEqual(x, y);
      Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
    }

    [Test]
    public void AnonymousTest()
    {
      Expression<Func<int, object>> x = n => new {Value = n};
      Expression<Func<int, object>> y = k => new {Value = k};
      var tx = x.ToExpressionTree();
      var ty = y.ToExpressionTree();
      Assert.AreEqual(tx, ty);
      Assert.AreEqual(tx.GetHashCode(), ty.GetHashCode());
    }

    [Test]
    public void ComplexExpressionTest()
    {
      Expression<Func<int, int, int>> sum = (a, b) => new { Result = a + b * 2 / a }.Result + DateTime.Now.Day * a * b - a + b;
      var x = sum.ToExpressionTree();
      var y = sum.ToExpressionTree();
      Assert.AreEqual(x, y);
      Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
    }

    [Test]
    public void UnboundParametersTest()
    {
      Expression<Func<int, int>> i1 = x => x + 1;
      Expression<Func<int, int>> i2 = x => x + 1;

      var p1 = i1.Parameters[0].ToExpressionTree();
      var p2 = i2.Parameters[0].ToExpressionTree();

      Assert.AreEqual(p1, p2);
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
    }

    [Test]
    public void NullEqualityTest()
    {
      Expression<Func<int, int>> expression = x => x + 1;
      var t = expression.ToExpressionTree();

      Assert.That(!t.Equals(null));

      Assert.That(null!=t);
      Assert.That(t!=null);

      Assert.That(!(t==null));
      Assert.That(!(null==t));

      t = null;

      Assert.That(t==null);
      Assert.That(null==t);
      Assert.That(!(t!=null));
      Assert.That(!(null!=t));
    }

    private static Expression<Func<int, int, int>> CreateProduct()
    {
      return (a, b) => a * b;
    }

    private static Expression<Func<int, int, int>> CreateSum()
    {
      return (a, b) => a + b;
    }

    private static Expression<Func<int, int, int>> CreateSum2()
    {
      return (x, y) => x + y;
    }
  }
}