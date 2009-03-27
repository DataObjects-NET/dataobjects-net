// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.25

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using System.Linq.Expressions;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Core.Linq.Normalization;


namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class ExpressionNormalizationTest
  {
    private int[] source = new[] { 1, 2, 3 };

    [Test]
    public void Test()
    {
      var qs = source.AsQueryable();

      IQueryable q = from i in qs where (i == 1) & !((i > 2) & ((i == 3) & (i == 4))) select i;
      Expression b = new BooleanSearcher().GetFirstBooleanExpression(q);
      Expression nb = new DisjunctiveNormalizer().Normalize(b).ToExpression();
      Expression nb1 = new DisjunctiveNormalizer(3).Normalize(b).ToExpression();

      DumpExpression(b);
      DumpExpression(nb);
      DumpExpression(nb1);
    }

    [Test]
    public void ConvertEqualsToDisjunction()
    {
      var eq = Expression.Equal(Expression.Constant(true), Expression.Constant(false));
      var expected = Expression.Or(
        Expression.And(Expression.Constant(true), Expression.Constant(false)), 
        Expression.And(Expression.Not(Expression.Constant(true)), Expression.Not(Expression.Constant(false))));
      var dis = new DisjunctiveNormalizer().Normalize(eq).ToExpression();

      Log.Info(eq.ToString());
      Log.Info(dis.ToString());
      Log.Info(expected.ToString());
      //Assert.AreEqual(expected, dis);
    }


    public void DumpExpression(Expression exp)
    {
      Log.Info(exp.ToString());
    }
  }

  public class BooleanSearcher : ExpressionVisitor
  {
    private Expression rootBoolean;

    public Expression GetFirstBooleanExpression(IQueryable query)
    {
      Visit(query.Expression);
      return rootBoolean;
    }

    protected override Expression Visit(Expression e)
    {
      if (e is BinaryExpression && rootBoolean == null)
        rootBoolean = e;

      return base.Visit(e);
    }
  }
}