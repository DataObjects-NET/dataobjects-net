// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Linq;

namespace Xtensive.Tests.Linq
{
  [TestFixture]
  public class ComparisonExtractorTest
  {
    [Test]
    public void SimpleExpressionTest()
    {
      int x = 10;
      Expression<Func<bool>> comparison = () => x > 10;
      string keyName = "x";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName, GetPartOfComparison(comparison, true),
        comparisonInfo);
    }

    [Test]
    public void BinaryExpressionAsRightPartTest()
    {
      int x = 10;
      int z = 1;
      Expression<Func<bool>> comparison = () => x > 10 - z;
      string keyName = "x";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName, GetPartOfComparison(comparison, true),
        comparisonInfo);
    }

    [Test]
    public void NegationTest()
    {
      int x = 10;
      int z = 1;
      Expression<Func<bool>> comparison = () => !(x > 10 - z);
      Expression valueExp = ((Expression<Func<int>>)(() => 10 - z)).Body;
      string keyName = "x";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x >= 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x < 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x <= 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x == 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x != 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(10 - z > x);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void RevertedComparisonTest()
    {
      int x = 10;
      int z = 1;
      Expression<Func<bool>> comparison = () => 10 - z > x;
      Expression valueExp = ((Expression<Func<int>>)(() => 10 - z)).Body;
      string keyName = "x";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName, valueExp,
        comparisonInfo);

      comparison = () => 10 - z >= x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z < x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z <= x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z == x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z != x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void CompareToMethodsTest()
    {
      int x = 10;
      int z = 1;
      Expression<Func<bool>> comparison = () => x.CompareTo(z) >= 0;
      Expression valueExp = ((Expression<Func<int>>) (() => z)).Body;
      string keyName = "x";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x.CompareTo(z) >= 0);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => x.CompareTo(z) == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => x.CompareTo(z) < 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      string xs = "abc";
      string zs = "ab";
      keyName = "xs";
      comparison = () => xs.CompareTo(zs) > 0;
      valueExp = ((Expression<Func<string>>) (() => zs)).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= 0;
      valueExp = ((Expression<Func<string>>)(() => zs)).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) != 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => zs.CompareTo(xs) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => zs.CompareTo(xs) <= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void CompareOrdinalTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => string.CompareOrdinal(xs, zs) < 0;
      Expression valueExp = ((Expression<Func<string>>)(() => zs)).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.LessThan, keyName,
        valueExp, ((BinaryExpression)comparison.Body).Left, comparisonInfo);

      // This overload is not supported.
      comparison = () => string.CompareOrdinal(xs, 0, zs, 0, 3) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      Assert.IsNull(comparisonInfo);
    }

    [Test]
    public void StaticCompareMethodsTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => string.Compare(xs, zs) < 0;
      Expression valueExp = ((Expression<Func<string>>) (() => zs)).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => string.Compare(xs, zs, true) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.LessThan, keyName,
        valueExp, ((BinaryExpression)comparison.Body).Left, comparisonInfo);

      comparison = () => string.Compare(zs, xs) >= 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => string.Compare(zs, xs, StringComparison.Ordinal) >= 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, ((BinaryExpression)comparison.Body).Left, comparisonInfo);

      comparison = () => 0 >= string.Compare(zs, xs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      DateTime xd = DateTime.Now;
      DateTime zd = new DateTime(2009, 03, 31);
      comparison = () => DateTime.Compare(xd, zd) <= 0;
      valueExp = ((Expression<Func<DateTime>>)(() => zd)).Body;
      keyName = "xd";
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void LikeMethodsTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => xs.StartsWith(zs);
      Expression valueExp = ((Expression<Func<string>>)(() => zs)).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LikeStartsWith, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.StartsWith(zs, StringComparison.Ordinal);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.LikeStartsWith, keyName,
        valueExp, comparison.Body, comparisonInfo);

      comparison = () => xs.EndsWith(zs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LikeEndsWith, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.EndsWith(zs, true, System.Globalization.CultureInfo.CurrentUICulture);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.LikeEndsWith, keyName,
        valueExp, comparison.Body, comparisonInfo);

      comparison = () => !xs.StartsWith(zs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotLikeStartsWith, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void EqualityComparisonMethodsTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => xs.Equals(zs + "t");
      Expression valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => !xs.Equals(zs + "t");
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.Equals(zs + "t", StringComparison.Ordinal);
      valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparison.Body, comparisonInfo);

      comparison = () => xs.Equals((object)(zs + "t"));
      valueExp = ((Expression<Func<object>>)(() => (object)(zs + "t"))).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => Equals(xs, (object)(zs + "t"));
      valueExp = ((Expression<Func<object>>)(() => (object)(zs + "t"))).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => !Equals(xs, (object)(zs + "t"));
      valueExp = ((Expression<Func<object>>)(() => (object)(zs + "t"))).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => string.Equals(xs, zs + "t");
      valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => !string.Equals(xs, zs + "t");
      valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => string.Equals(xs, zs + "t", StringComparison.Ordinal);
      valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparison.Body, comparisonInfo);

      comparison = () => !string.Equals(xs, zs + "t", StringComparison.Ordinal);
      valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckComplexComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, ((UnaryExpression)comparison.Body).Operand, comparisonInfo);

      DateTime xd = DateTime.Now;
      DateTime zd = new DateTime(2009, 4, 1);

      comparison = () => xd.Equals((object)(zd));
      valueExp = ((Expression<Func<object>>)(() => zd)).Body;
      keyName = "xd";
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => DateTime.Equals(xd, zd);
      valueExp = ((Expression<Func<DateTime>>)(() => zd)).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void BooleanTest()
    {
      bool xb = true;
      bool zb = false;
      Expression<Func<bool>> comparison = () => xb == zb;
      Expression valueExp = ((Expression<Func<bool>>)(() => zb)).Body;
      string keyName = "xb";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => xb != zb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !xb == zb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xb == !zb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        ((Expression<Func<bool>>)(() => !zb)).Body, comparisonInfo);

      comparison = () => zb != !xb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => xb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        Expression.Constant(true), comparisonInfo);

      comparison = () => !xb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.NotEqual, keyName,
        Expression.Constant(true), comparisonInfo);

      comparison = () => !!xb;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.Equal, keyName,
        Expression.Constant(true), comparisonInfo);
    }

    [Test]
    public void ForcedComparisonMethodsTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => xs.GreaterThan(zs + "t");
      Expression valueExp = ((Expression<Func<string>>)(() => zs + "t")).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => (zs + "t").GreaterThan(xs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.GreaterThanOrEqual(zs + "t");
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.LessThan(zs + "t");
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => (zs + "t").LessThan(xs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.LessThanOrEqual(zs + "t");
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonOperation.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void InvalidExpressionsTest()
    {
      string xs = "abc";
      string zs = "ab";
      var keyName = "xs";

      // The key will not be found.
      Expression<Func<bool>> comparison = () => xs.CompareTo(zs) + 1 == 0;
      var comparisonInfo = ExtractComparisonInfo(comparison, "t");
      Assert.IsNull(comparisonInfo);

      // The left part contains BinaryExpression.
      comparison = () => xs.CompareTo(zs) + 1 == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      Assert.IsNull(comparisonInfo);

      // The key is a part of BinaryExpression.
      comparison = () => zs.CompareTo(xs + "t") + 1 == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      Assert.IsNull(comparisonInfo);

      bool xb = true;
      bool zb = false;
      // The key is a part of UnaryExpression.
      comparison = () => (!xb).CompareTo(zb) == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, "xb");
      Assert.IsNull(comparisonInfo);

      int xi = 1;
      int zi = 2;

      // The expression contains more than one comparison operation.
      comparison = () => (xi > zi) == false;
      comparisonInfo = ExtractComparisonInfo(comparison, "xi");
      Assert.IsNull(comparisonInfo);

      // The expression does not contain any comparison operations.
      comparison = () => true; // (xi + zi) is int;
      comparisonInfo = ExtractComparisonInfo(comparison, "xi");
      Assert.IsNull(comparisonInfo);

      // The result of the expression is not bool.
      comparisonInfo = ExtractComparisonInfo((Expression<Func<int>>)(() => (xi + zi)), "xi");
      Assert.IsNull(comparisonInfo);

      // The expression does not contain any comparison methods.
      comparison = () => string.IsNullOrEmpty(xs);
      comparisonInfo = ExtractComparisonInfo(comparison, "xi");
      Assert.IsNull(comparisonInfo);

      // The expression contains key in both of operands.
      comparison = () => xi > xi + 1;
      comparisonInfo = ExtractComparisonInfo(comparison, "xi");
      Assert.IsNull(comparisonInfo);

      // The expression contains key in both of operands.
      comparison = () => xi.CompareTo(xi + 1) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, "xi");
      Assert.IsNull(comparisonInfo);
    }

    private static ComparisonInfo ExtractComparisonInfo(Expression comparison,
      string keyName)
    {
      ComparisonExtractor extractor = new ComparisonExtractor();
      return extractor.Extract(comparison, exp =>
      {
        var memberExp = exp as MemberExpression;
        return memberExp != null && memberExp.Member.Name == keyName;
      });
    }

    private static void CheckSimpleComparison(ComparisonOperation comparisonOperation, string keyName, Expression value,
      ComparisonInfo result)
    {
      CheckComparison(comparisonOperation, keyName, value, false, result);
    }

    private static void CheckComplexComparison(ComparisonOperation comparisonOperation, string keyName,
      Expression value, Expression complexMethod, ComparisonInfo result)
    {
      CheckComparison(comparisonOperation, keyName, value, true, result);
      Assert.AreEqual(complexMethod.ToString(true), result.ComplexMethod.ToString(true));
    }

    private static void CheckComparison(ComparisonOperation comparisonOperation, string keyName,
      Expression value, bool isComlex, ComparisonInfo result)
    {
      Assert.IsNotNull(result);
      Assert.AreEqual(comparisonOperation, result.Operation);
      Assert.AreEqual(isComlex, result.IsComplex);
      if(!isComlex)
        Assert.IsNull(result.ComplexMethod);
      else
        Assert.IsNotNull(result.ComplexMethod);
      Assert.AreEqual(keyName, ((MemberExpression)result.Key).Member.Name);
      Assert.AreEqual(value.ToString(true), result.Value.ToString(true));
    }

    private static Expression GetPartOfComparison(LambdaExpression comparison, bool returnRight)
    {
      var binary = (BinaryExpression) comparison.Body;
      return returnRight ? binary.Right : binary.Left;
    }
  }
}