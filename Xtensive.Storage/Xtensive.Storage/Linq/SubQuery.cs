// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Materialization;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class SubQuery<TElement> : 
    IOrderedQueryable<TElement>, 
    IOrderedEnumerable<TElement>
  {
    private readonly ProjectionExpression projectionExpression;
    private FutureSequence<TElement> futureSequence;
    private List<TElement> materializedSequence;
    private readonly QueryProvider provider;

    public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
      throw new NotSupportedException();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      if (materializedSequence == null)
        materializedSequence = futureSequence.ToList();
      return materializedSequence.GetEnumerator();
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
      get { return provider; }
    }

    private void MaterializeSelf()
    {
      if (materializedSequence != null) 
        return;
      materializedSequence = futureSequence.ToList();
      futureSequence = null;
    }


    // Constructors

    public SubQuery(ProjectionExpression projectionExpression, TranslatedQuery query, Parameter<Tuple> parameter, Tuple tuple, ItemMaterializationContext context)
    {
      this.provider = context.Session.Query.Provider;
      var tupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(projectionExpression.TupleParameterBindings);
      var currentTranslatedQuery = ((TranslatedQuery<IEnumerable<TElement>>) query);

      // Gather Parameter<Tuple> values from current ParameterScope for future use. 
      parameter.Value = tuple;
      foreach (var tupleParameter in currentTranslatedQuery.TupleParameters) {
        var value = tupleParameter.Value;
        tupleParameterBindings[tupleParameter] = value;
      }
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate())
      foreach (var tupleParameter in currentTranslatedQuery.TupleParameters)
        tupleParameter.Value = tupleParameter.Value;

      this.projectionExpression = new ProjectionExpression(
        projectionExpression.Type, 
        projectionExpression.ItemProjector, 
        tupleParameterBindings, 
        projectionExpression.ResultType);
      var translatedQuery = new TranslatedQuery<IEnumerable<TElement>>(
        query.DataSource,
        (Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, IEnumerable<TElement>>) query.UntypedMaterializer,
        tupleParameterBindings,
        EnumerableUtils<Parameter<Tuple>>.Empty);
      futureSequence = new FutureSequence<TElement>(translatedQuery, parameterContext);
      context.Session.RegisterDelayedQuery(futureSequence.Task);
      context.MaterializationContext.MaterializationQueue.Enqueue(MaterializeSelf);
    }
  }
}