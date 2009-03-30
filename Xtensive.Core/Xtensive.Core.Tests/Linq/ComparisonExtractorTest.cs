// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq.ComparisonExtraction;

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
      ComparisonExtractor extractor = new ComparisonExtractor();
      var comparisonInfo = extractor.Extract(comparison, (exp) => {
        var memberExp = exp as MemberExpression;
        return memberExp!=null && memberExp.Member.Name=="x";
      });
      Assert.IsNotNull(comparisonInfo);
    }
  }
}