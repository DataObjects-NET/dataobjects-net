// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Linq.Expressions;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// A compiler for <see cref="Expression"/> that caches compilation results.
  /// </summary>
  public sealed class CachingExpressionCompiler
  {
    private static readonly object _lock = new object();
    private static volatile CachingExpressionCompiler defaultInstance = new CachingExpressionCompiler();

    /// <summary>
    /// Gets the default <see cref="CachingExpressionCompiler"/>.
    /// </summary>
    /// <value></value>
    public static CachingExpressionCompiler Default {
      get {
        if (defaultInstance==null)
          lock (_lock)
            if (defaultInstance==null)
              defaultInstance = new CachingExpressionCompiler();
        return defaultInstance;
      }
    }

    private readonly ThreadSafeDictionary<ExpressionTree, Delegate> cache =
      ThreadSafeDictionary<ExpressionTree, Delegate>.Create(new object());

    /// <summary>
    /// Compiles specified <see cref="LambdaExpression"/> and returns a pair of <see cref="Delegate"/> and an object array.
    /// Delegate is a compiled version of <paramref name="lambda"/> with one extra argument (at first position).
    /// Array contains extracted constants and should be passed as first argument to returned delegate
    /// to get the same behavior as original lambda.
    /// </summary>
    /// <param name="lambda">An expression to compile.</param>
    /// <returns>Result of compilation.</returns>
    public Pair<Delegate, object[]> RawCompile(LambdaExpression lambda)
    {
      var constantExtractor = new ConstantExtractor(lambda);
      var tree = new ExpressionTree(constantExtractor.Process());
      var constants = constantExtractor.GetConstants();
      var _delegate = cache.GetValue(tree, _tree => ((LambdaExpression) _tree.Expression).Compile());
      return new Pair<Delegate, object[]>(_delegate, constants);
    }
  }
}