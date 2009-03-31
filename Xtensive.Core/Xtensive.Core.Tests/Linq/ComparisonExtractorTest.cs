// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq.ComparisonExtraction;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Tests.Linq
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
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName, GetPartOfComparison(comparison, true),
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
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName, GetPartOfComparison(comparison, true),
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
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x >= 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x < 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x <= 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x == 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(x != 10 - z);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => !(10 - z > x);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
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
      CheckSimpleComparison(ComparisonType.LessThan, keyName, valueExp,
        comparisonInfo);

      comparison = () => 10 - z >= x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z < x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z <= x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z == x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => 10 - z != x;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.NotEqual, keyName,
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
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => x.CompareTo(z) == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => x.CompareTo(z) < 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      string xs = "abc";
      string zs = "ab";
      comparison = () => xs.CompareTo(zs) > 0;
      valueExp = ((Expression<Func<string>>) (() => zs)).Body;
      keyName = "xs";
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= 0;
      valueExp = ((Expression<Func<string>>)(() => zs)).Body;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.Equal, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) != 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.NotEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => zs.CompareTo(xs) < 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= -1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) >= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) == 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => xs.CompareTo(zs) <= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => zs.CompareTo(xs) <= 1;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);
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
      CheckSimpleComparison(ComparisonType.LessThan, keyName,
        valueExp, comparisonInfo);

      comparison = () => string.Compare(zs, xs) >= 0;
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);

      comparison = () => 0 >= string.Compare(zs, xs);
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.GreaterThanOrEqual, keyName,
        valueExp, comparisonInfo);

      DateTime xd = DateTime.Now;
      DateTime zd = new DateTime(2009, 03, 31);
      comparison = () => DateTime.Compare(xd, zd) <= 0;
      valueExp = ((Expression<Func<DateTime>>)(() => zd)).Body;
      keyName = "xd";
      comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.LessThanOrEqual, keyName,
        valueExp, comparisonInfo);
    }

    [Test]
    public void LikeMethodsTest()
    {
      string xs = "abc";
      string zs = "ab";
      Expression<Func<bool>> comparison = () => xs.StartsWith(zs);
      Expression valueExp = ((Expression<Func<string>>)(() => String.Format("{0}%", zs))).Body;
      string keyName = "xs";
      var comparisonInfo = ExtractComparisonInfo(comparison, keyName);
      CheckSimpleComparison(ComparisonType.Like, keyName,
        valueExp, comparisonInfo);
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

    private static void CheckSimpleComparison(ComparisonType comparisonType, string keyName, Expression value,
      ComparisonInfo result)
    {
      Assert.IsNotNull(result);
      Assert.AreEqual(comparisonType, result.Operation);
      Assert.AreEqual(false, result.IsComplex);
      Assert.IsNull(result.ComplexMethod);
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