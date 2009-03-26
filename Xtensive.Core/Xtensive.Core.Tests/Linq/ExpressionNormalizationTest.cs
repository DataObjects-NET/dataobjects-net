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
      var n = new DisjunctiveNormalizer();

      IQueryable q = from i in qs where (i == 1) & !((i == 2) & ((i == 3) & (i == 4))) select i;
      string expected = "((i = 1) And Not(i = 2)) Or (((i = 1) And Not(i = 3)) Or ((i = 1) And Not(i = 4)))";
      Expression b = new BooleanSearcher().GetFirstBooleanExpression(q);
      Expression nb = n.Normalize(b);

      DumpExpression(b);
      DumpExpression(nb);
      Log.Info(expected);
      
      Assert.AreEqual(expected, nb);
    }

    public void DumpQuery(IQueryable query)
    {
      Log.Info(query.Expression.ToString());
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