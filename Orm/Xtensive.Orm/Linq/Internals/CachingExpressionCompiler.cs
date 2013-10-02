// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Linq
{
  internal sealed class CachingExpressionCompiler
  {
    private static readonly object _lock = new object();
    private static volatile CachingExpressionCompiler instance;

    public static CachingExpressionCompiler Instance {
      get {
        if (instance==null) lock (_lock) if (instance==null)
          instance = new CachingExpressionCompiler();
        return instance;
      }
    }

    private readonly ThreadSafeDictionary<ExpressionTree, Delegate> cache =
      ThreadSafeDictionary<ExpressionTree, Delegate>.Create(new object());

    public Pair<Delegate, object[]> Compile(LambdaExpression lambda)
    {
      var constantExtractor = new ConstantExtractor(lambda);
      var tree = constantExtractor.Process().ToExpressionTree();
      var constants = constantExtractor.GetConstants();
      var compiled = cache.GetValue(tree, _tree => ((LambdaExpression) _tree.ToExpression()).Compile());
//      var compiled = ((LambdaExpression) tree.ToExpression()).Compile();
      return new Pair<Delegate, object[]>(compiled, constants);
    }

    // For testing only
    public void ClearCache()
    {
      cache.Clear();
    }


    // Constructors

    private CachingExpressionCompiler()
    {
    }
  }
}