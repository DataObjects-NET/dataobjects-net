// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  [Explicit]
  [Category("Performance")]
  public class CachingExpressionCompilerTest
  {
    [Test]
    public void SimpleExpressionPerformanceTest()
    {
      Expression<Func<int, int, int>> lambda = (a, b) => a + b;
      RunCompileTest(lambda, true, 10);
      RunCompileTest(lambda, false, 10000);
    }

    [Test]
    public void ComplexExpressionPerformanceTest()
    {
      Expression<Func<int, int, int>> lambda =
        (a, b) => new { Result = a + b * 2 / a }.Result + DateTime.Now.Day * a * b - a + b;
      RunCompileTest(lambda, true, 10);
      RunCompileTest(lambda, false, 10000);
    }

    private static void RunCompileTest(Expression<Func<int, int, int>> lambda, bool warmup, int operationCount)
    {
      var compiler = new CachingExpressionCompiler();
      using (CreateMeasurement(warmup, "Without caching: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          lambda.Compile();
      using (CreateMeasurement(warmup, "With caching: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          compiler.Compile(lambda);
    }

    private static IDisposable CreateMeasurement(bool warmup, string name, int operationCount)
    {
      if (warmup) {
        Log.Info("Warming up...");
        return null;
      }
      return new Measurement(name, operationCount);
    }
  }
}