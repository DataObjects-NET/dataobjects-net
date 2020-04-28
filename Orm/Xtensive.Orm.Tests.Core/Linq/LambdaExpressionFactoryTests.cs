// Licensed to the Xtensive LLC under one or more agreements.
// The Xtensive LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [TestFixture]
  public class LambdaExpressionFactoryTests
  {
    private readonly Type delegateType = typeof(Func<int, int>);
    private readonly Expression<Func<int, int>> plusOne = i => i + 1;

    [Test]
    public void ShouldUseFastFactory() => Assert.That(LambdaExpressionFactory.CanUseFastFactory());

    [Test]
    public void ShouldCreateFactoryFast()
    {
      var lambda = (Func<int, int>) LambdaExpressionFactory
        .CreateFactoryFast(delegateType)
        .Invoke(plusOne.Body, plusOne.Parameters.ToArray())
        .Compile();

      Assert.That(lambda.Invoke(1), Is.EqualTo(2));
    }

    [Test]
    public void ShouldCreateFactorySlow()
    {
      var lambda = (Func<int, int>) LambdaExpressionFactory.Instance
        .CreateFactorySlow(delegateType)
        .Invoke(plusOne.Body, plusOne.Parameters.ToArray())
        .Compile();

      Assert.That(lambda.Invoke(1), Is.EqualTo(2));
    }

    [Test]
    public void ShouldCreateLambda()
    {
      var lambda = (Func<int, int>) LambdaExpressionFactory.Instance
        .CreateLambda(delegateType, plusOne.Body, plusOne.Parameters.ToArray())
        .Compile();

      Assert.That(lambda.Invoke(1), Is.EqualTo(2));
    }
  }
}