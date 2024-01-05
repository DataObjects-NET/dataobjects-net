// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System.Collections.Concurrent;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class CachingExpressionCompiler
  {
    public static CachingExpressionCompiler Instance { get; } = new CachingExpressionCompiler();

    private readonly ConcurrentDictionary<ExpressionTree, Delegate> cache =
      new ConcurrentDictionary<ExpressionTree, Delegate>();

    private static readonly Func<ExpressionTree, Delegate> expressionTreeCompiler = CompileExpressionTree;

    private static Delegate CompileExpressionTree(ExpressionTree tree) =>
      ((LambdaExpression) tree.ToExpression()).Compile();

    public Pair<Delegate, object[]> Compile(LambdaExpression lambda)
    {
      var constantExtractor = new ConstantExtractor(lambda);
      var expressionTree = constantExtractor.Process().ToExpressionTree();
      var constants = constantExtractor.GetConstants();

      var compiled = cache.GetOrAdd(expressionTree, expressionTreeCompiler);
      return new Pair<Delegate, object[]>(compiled, constants);
    }

    // For testing only
    public void ClearCache() => cache.Clear();


    // Constructors

    private CachingExpressionCompiler()
    {
    }
  }
}