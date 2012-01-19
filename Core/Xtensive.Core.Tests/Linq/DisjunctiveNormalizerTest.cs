// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.25

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Linq;
using Xtensive.Linq.Normalization;
using Xtensive.Testing;
using ExpressionExtensions = Xtensive.Core.ExpressionExtensions;

namespace Xtensive.Tests.Linq
{
  [TestFixture]
  public class DisjunctiveNormalizerTest
  {
    [Test]
    public void Complex0Test()
    {
      for (var i = 0; i < Math.Pow(2, 4); i++) {
        var terms = new BitArray(new[] {i});
        var exp = Expression.Not(
          Expression.Or(
            Expression.And(
              Expression.Constant(terms[0]),
              Expression.Constant(terms[1])),
            Expression.And(
              Expression.Constant(terms[2]),
              Expression.Constant(terms[3]))));
        var normalizedExp = new DisjunctiveNormalizer().Normalize(exp);
        NormalizerTest(exp, normalizedExp);
      }
    }

    [Test]
    public void Complex1Test()
    {
      for (var i = 0; i < Math.Pow(2, 4); i++) {
        var terms = new BitArray(new[] {i});
        var exp = Expression.AndAlso(
          Expression.OrElse(
            Expression.Constant(terms[0]),
            Expression.Constant(terms[1])),
          Expression.Not(
            Expression.Equal(
              Expression.Constant(terms[2]),
              Expression.LessThan(
                Expression.Constant(i),
                Expression.Constant(8)))));
        var normalizedExp = new DisjunctiveNormalizer().Normalize(exp);
        NormalizerTest(exp, normalizedExp);
      }
    }

    [Test]
    public void Complex2Test()
    {
      for (var i = 0; i < Math.Pow(2, 8); i++)
      {
        var terms = new BitArray(new[] { i });
        var exp = Expression.And(
          Expression.Or(
            Expression.Constant(terms[0]),
            Expression.And(
              Expression.Constant(terms[1]),
              Expression.Or(
                Expression.Constant(terms[2]),
                Expression.Constant(terms[3])))),
          Expression.Or(
            Expression.Constant(terms[4]),
            Expression.And(
              Expression.Constant(terms[5]),
              Expression.Or(
                Expression.Constant(terms[6]),
                Expression.Constant(terms[7])))));
        var normalizedExp = new DisjunctiveNormalizer().Normalize(exp);
        NormalizerTest(exp, normalizedExp);
      }
    }

    [Test]
    public void NormalizeEqualsTest()
    {
      var exp = Expression.Equal(Expression.Constant(true), Expression.Constant(false));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(true),
              Expression.Constant(false)
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Not(Expression.Constant(true)),
              Expression.Not(Expression.Constant(false))
            })
        });

      NormalizerTest(expected, exp);
    }

    [Test]
    public void NormalizeNotNotTest()
    {
      var exp = Expression.Not(
        Expression.Not(
          Expression.Or(
            Expression.Constant(true),
            Expression.Constant(false))));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(true),
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(false)
            })
        });

      NormalizerTest(expected, exp);
    }

    [Test]
    public void NormalizeNotEqualsTest()
    {
      var exp = Expression.NotEqual(Expression.Constant(true), Expression.Constant(false));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new Expression[]
            {
              Expression.Not(Expression.Constant(true)),
              Expression.Constant(false)
            }),
          new Conjunction<Expression>(new Expression[]
            {
              Expression.Constant(true),
              Expression.Not(Expression.Constant(false))
            })
        });

      NormalizerTest(expected, exp);
    }

    [Test]
    public void NormalizeInversionTest()
    {
      var exp = Expression.Not(
        Expression.Equal(Expression.Constant(true), Expression.Constant(false)));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new Expression[]
            {
              Expression.Not(Expression.Constant(true)),
              Expression.Constant(false)
            }),
          new Conjunction<Expression>(new Expression[]
            {
              Expression.Constant(true),
              Expression.Not(Expression.Constant(false))
            })
        });
      NormalizerTest(expected, exp);

      exp = Expression.Not(
        Expression.NotEqual(Expression.Constant(true), Expression.Constant(false)));
      expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(true),
              Expression.Constant(false)
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Not(Expression.Constant(true)),
              Expression.Not(Expression.Constant(false))
            })
        });
      NormalizerTest(expected, exp);

      exp = Expression.Not(Expression.Or(Expression.Constant(true), Expression.Constant(false)));
      expected = new DisjunctiveNormalized(
          new Conjunction<Expression>(new[]
            {
              Expression.Not(Expression.Constant(true)),
              Expression.Not(Expression.Constant(false))
            }));
      NormalizerTest(expected, exp);

      exp = Expression.Not(Expression.And(Expression.Constant(true), Expression.Constant(false)));
      expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(Expression.Not(Expression.Constant(true))),
          new Conjunction<Expression>(Expression.Not(Expression.Constant(false)))
        });
      NormalizerTest(expected, exp);
    }

    [Test]
    public void NormalizeDisjunctionTest()
    {
      var exp = Expression.Or(
        Expression.Or(Expression.Constant(true), Expression.Constant(false)),
        Expression.Or(Expression.Constant(true), Expression.Constant(false)));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(Expression.Constant(true)),
          new Conjunction<Expression>(Expression.Constant(false)),
          new Conjunction<Expression>(Expression.Constant(true)),
          new Conjunction<Expression>(Expression.Constant(false))
        });

      NormalizerTest(expected, exp);
    }

    [Test]
    public void NormalizeConjunctionTest()
    {
      var exp = Expression.And(
        Expression.Or(Expression.Constant(true), Expression.Constant(false)),
        Expression.Or(Expression.Constant(true), Expression.Constant(false)));
      var expected = new DisjunctiveNormalized(new[]
        {
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(true),
              Expression.Constant(true)
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(true),
              Expression.Constant(false)
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(false),
              Expression.Constant(true)
            }),
          new Conjunction<Expression>(new[]
            {
              Expression.Constant(false),
              Expression.Constant(false)
            })
        });

      NormalizerTest(expected, exp);
    }

    [Test]
    public void MaxConjunctionOperandCountTest()
    {
      var exp = Expression.And(
        Expression.Or(
          Expression.Or(
            Expression.Or(
              Expression.Constant(true),
              Expression.Constant(false)),
            Expression.Constant(false)),
          Expression.Constant(false)),
        Expression.Or(
          Expression.Constant(true),
          Expression.Constant(false)));
      
      AssertEx.Throws<InvalidOperationException>(() => 
        new DisjunctiveNormalizer(3).Normalize(exp));
    }

    [Test]
    public void LambdaNormalizationTest()
    {
      Expression<Func<bool>> exp = () => (true && false) || (false && false);
      var normalizedExp = new DisjunctiveNormalizer().Normalize(exp);
      var actual = Expression.Lambda<Func<bool>>(normalizedExp.ToExpression()).Compile()();
      Assert.AreEqual(exp.Compile()(), actual);
    }
    
    public void NormalizerTest(Expression expression, DisjunctiveNormalized normalized)
    {
      var expected = Expression.Lambda<Func<bool>>(expression).Compile()();
      var actual = Expression.Lambda<Func<bool>>(normalized.ToExpression()).Compile()();
      Log.Info("{0} = {1}", ExpressionExtensions.ToString(expression, true), expected);
      Log.Info("{0} = {1}", normalized.ToString(true), actual);
      Assert.AreEqual(expected, actual);
    }

    public void NormalizerTest(DisjunctiveNormalized expected, Expression toNormalize)
    {
      var normalized = new DisjunctiveNormalizer().Normalize(toNormalize);

      Log.Info("Expression: {0}", ExpressionExtensions.ToString(toNormalize, true));
      Log.Info("Normalized: {0}", normalized.ToString(true));
      Log.Info("Expected:   {0}", expected.ToString(true));
      Assert.AreEqual(expected.ToString(true), normalized.ToString(true));
    }
  }

  internal static class DisjunctiveNormalizeExtensions
  {
    internal static string ToString(this DisjunctiveNormalized exp, bool inCspNotation)
    {
      return inCspNotation
        ? exp.Operands.Select(
          c => c.Operands.Select(
            e => ExpressionExtensions.ToString(e, true))
            .Join(" & ", "({0})"))
          .Join(" | ", "{0}")
        : exp.ToString();
    }

    private static string Join(this IEnumerable<string> strings, string separator, string format)
    {
      return string.Format(format, string.Join(separator, strings.ToArray()));
    }
  }
}