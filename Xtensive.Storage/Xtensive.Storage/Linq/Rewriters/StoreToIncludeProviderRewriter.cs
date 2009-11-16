// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.13

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class StoreToIncludeProviderRewriter : CompilableProviderVisitor
  {
    protected override Provider VisitApply(ApplyProvider applyProvider)
    {
      if (applyProvider.Right.Type==ProviderType.Existence) {
        var existenceProvider = (ExistenceProvider) applyProvider.Right;
        if (existenceProvider.Source.Type==ProviderType.Filter) {
          var innerFilterProvider = (FilterProvider) existenceProvider.Source;
          var mappingGatherer = new ExtendedExpressionReplacer(delegate(Expression ex) {
            if (ex.NodeType==ExpressionType.Equal) {
              var equalsExpression = (BinaryExpression) ex;
              var left = equalsExpression.Left.StripCasts();
              var right = equalsExpression.Right.StripCasts();
              if (left.NodeType==ExpressionType.Call) {
                var leftCall = (MethodCallExpression) left;
                if (leftCall.Object==innerFilterProvider.Predicate.Parameters[0] 
                  && leftCall.Method.IsGenericMethod 
                  && leftCall.Method.GetGenericMethodDefinition()==WellKnownMembers.Tuple.GenericAccessor 
                  && leftCall.Arguments.Count == 1) {
                  var leftIndex = leftCall.Arguments;
                  if (right.NodeType==ExpressionType.Call) {
                    throw new NotImplementedException();
                  }
                }
              }
            }
            return null;
          });
          mappingGatherer.Replace(innerFilterProvider.Predicate);
          // TODO: Check filter.
          if (innerFilterProvider.Source.Type==ProviderType.Store) {
            var storeProvider = (StoreProvider) innerFilterProvider.Source;
            if (storeProvider.Source.Type==ProviderType.Raw) {
              var rawProvider = (RawProvider) storeProvider.Source;
              var tupleEnumerable = rawProvider.Source;
              var includeProvider = new IncludeProvider(applyProvider.Left, IncludeAlgorithm.Auto, false, tupleEnumerable, existenceProvider.ExistenceColumnName, null);
              return includeProvider;
            }
          }
        }
      }
      return base.VisitApply(applyProvider);
    }

    public static ProjectionExpression Rewrite(ProjectionExpression expression)
    {
      var visitor = new StoreToIncludeProviderRewriter();
      var newProvider = (CompilableProvider) visitor.Visit(expression.ItemProjector.DataSource.Provider);
      if (newProvider==expression.ItemProjector.DataSource.Provider)
        return expression;
      var newItemProjector = expression.ItemProjector.Remap(newProvider.Result, 0);
      return new ProjectionExpression(expression.Type, newItemProjector, expression.TupleParameterBindings, expression.ResultType);
    }

    private StoreToIncludeProviderRewriter()
    {
    }
  }
}