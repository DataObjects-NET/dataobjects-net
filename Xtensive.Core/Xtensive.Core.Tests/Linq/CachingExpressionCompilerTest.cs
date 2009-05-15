// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class CachingExpressionCompilerTest
  {
    [Test]
    public void InterfaceImplicitCastTest()
    {
      Expression<Func<int, IEnumerable>> lambda = n => Enumerable.Range(1, n);
      lambda.CompileCached();
    }

    #region Performance testing

    private const int warpUpOperationCount = 1;
    private const int actualOperationCount = 1000;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void SimpleExpressionPerformanceTest()
    {
      Expression<Func<int, int, int>> lambda = (a, b) => a + b;
      RunCompilePerformanceTest(lambda, true);
      RunCompilePerformanceTest(lambda, false);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void ComplexExpressionPerformanceTest()
    {
      Expression<Func<int, int, int>> lambda =
        (a, b) => new {Result = a + b * 2 / a}.Result + DateTime.Now.Day * a * b - a + b;
      RunCompilePerformanceTest(lambda, true);
      RunCompilePerformanceTest(lambda, false);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void CombinedTest()
    {
      Expression<Func<int, int, int>> lambda =
        (a, b) => new {Result = a + b * 2 / (a + 1)}.Result + DateTime.Now.Day * a * b - a + b;
      RunCompileAndInvokePerformanceTest(lambda, true);
      RunCompileAndInvokePerformanceTest(lambda, false);
    }

    [Test]
    [Ignore("Results depend on order of calling")]
    [Category("Performance")]
    public void CallOverheadTest()
    {
      var parameter = Expression.Parameter(typeof (int), "p");
      Expression<Func<int, int>> plusOne = Expression.Lambda<Func<int, int>>(
        Expression.Add(parameter, Expression.Constant(1)), parameter);

      ClearCompilerCache();
      var original = plusOne.Compile();
      var cached = plusOne.CompileCached();
      
      RunCallOverheadTest(original, cached, true);
      RunCallOverheadTest(original, cached, false);
    }

    private static void RunCallOverheadTest(Func<int, int> original, Func<int, int> cached, bool warmUp)
    {
      int operationCount = warmUp ? warpUpOperationCount : actualOperationCount;
      int k = 0;

      k = 0;
      using (CreateMeasurement(warmUp, "Call cached: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          k = cached.Invoke(k);

      k = 0;
      using (CreateMeasurement(warmUp, "Call original: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          k = original.Invoke(k);
    }

    private static void RunCompilePerformanceTest(Expression<Func<int, int, int>> lambda, bool warmUp)
    {
      int operationCount = warmUp ? warpUpOperationCount : actualOperationCount;
      using (CreateMeasurement(warmUp, "Without caching: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          lambda.Compile();
      ClearCompilerCache();
      using (CreateMeasurement(warmUp, "With caching: ", operationCount))
        for (int i = 0; i < operationCount; i++)
          lambda.CompileCached();
    }

    private static void RunCompileAndInvokePerformanceTest(Expression<Func<int, int, int>> lambda, bool warmUp)
    {
      int operationCount = warmUp ? warpUpOperationCount : actualOperationCount;
      using (CreateMeasurement(warmUp, "Without caching: ", operationCount))
        for (int i = 0; i < operationCount; i++) {
          var func = lambda.Compile();
          func(i, i);
        }

      ClearCompilerCache();
      using (CreateMeasurement(warmUp, "With caching: ", operationCount))
        for (int i = 0; i < operationCount; i++) {
          var func = lambda.CompileCached();
          func(i, i);
        }
    }

    private static void ClearCompilerCache()
    {
      var type = typeof(Pair<>).Assembly.GetType("Xtensive.Core.Linq.Internals.CachingExpressionCompiler");
      var instance = type.InvokeMember("Instance",
        BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty,
        null, null, ArrayUtils<object>.EmptyArray);
      type.InvokeMember("ClearCache",
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
        null, instance, ArrayUtils<object>.EmptyArray);
    }

    private static IDisposable CreateMeasurement(bool warmUp, string name, int operationCount)
    {
      return warmUp
        ? new Measurement(name, MeasurementOptions.None, operationCount)
        : new Measurement(name, operationCount);
    }

    #endregion
  }
}