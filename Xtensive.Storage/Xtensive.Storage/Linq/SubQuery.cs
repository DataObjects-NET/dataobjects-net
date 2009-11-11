// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Materialization;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class SubQuery<TElement> : FutureBase<IEnumerable<TElement>>,
    IOrderedQueryable<TElement>, 
    IOrderedEnumerable<TElement>
  {
    private readonly ProjectionExpression projectionExpression;

    public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
      throw new NotSupportedException();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      return Materialize().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Expression Expression
    {
      get { return projectionExpression; }
    }

    public Type ElementType
    {
      get { return typeof (TElement); }
    }

    public IQueryProvider Provider
    {
      get { return QueryProvider.Instance; }
    }


    // Constructors

    public SubQuery(ProjectionExpression projectionExpression, TranslatedQuery translatedQuery, Parameter<Tuple> parameter, Tuple tuple, ItemMaterializationContext context)
      : base((TranslatedQuery<IEnumerable<TElement>>)translatedQuery, new ParameterContext())
    {
      this.projectionExpression = new ProjectionExpression(projectionExpression.Type, projectionExpression.ItemProjector, projectionExpression.TupleParameterBindings, projectionExpression.ResultType);
      var query = ((TranslatedQuery<IEnumerable<TElement>>) translatedQuery);

      // Gather Parameter<Tuple> values from current ParameterScope for future use. 
      parameter.Value = tuple;
      foreach (var tupleParameter in query.TupleParameters) {
        var value = tupleParameter.Value;
        this.projectionExpression.TupleParameterBindings[tupleParameter] = value;
        tupleParameterBindings[tupleParameter] = value;
      }

      using (Task.ParameterContext.ActivateSafely())
      foreach (var tupleParameter in query.TupleParameters)
        tupleParameter.Value = tupleParameter.Value;
      context.Session.RegisterDelayedQuery(Task);
    }
  }
}