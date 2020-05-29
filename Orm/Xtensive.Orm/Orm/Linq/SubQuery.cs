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
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class SubQuery<TElement> : 
    IOrderedQueryable<TElement>, 
    IOrderedEnumerable<TElement>
  {
    private readonly ProjectionExpression projectionExpression;
    private DelayedSequence<TElement> delayedSequence;
    private List<TElement> materializedSequence;
    private readonly QueryProvider provider;

    public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
      throw new NotSupportedException();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      if (materializedSequence == null)
        materializedSequence = delayedSequence.ToList();
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
      materializedSequence = delayedSequence.ToList();
      delayedSequence = null;
    }


    // Constructors

// ReSharper disable MemberCanBeProtected.Global
    public SubQuery(ProjectionExpression projectionExpression, TranslatedQuery query, Parameter<Tuple> parameter, Tuple tuple, ItemMaterializationContext context)
// ReSharper restore MemberCanBeProtected.Global
    {
      this.provider = context.Session.Query.Provider;
      var tupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(projectionExpression.TupleParameterBindings);
      var currentTranslatedQuery = query;

      var outerParameterContext = context.ParameterContext;
      var parameterContext = new ParameterContext(outerParameterContext);
      // Gather Parameter<Tuple> values from current ParameterScope for future use.
      outerParameterContext.SetValue(parameter, tuple);
      foreach (var tupleParameter in currentTranslatedQuery.TupleParameters) {
        var value = outerParameterContext.GetValue(tupleParameter);
        tupleParameterBindings[tupleParameter] = value;
        parameterContext.SetValue(tupleParameter, value);
      }

      this.projectionExpression = new ProjectionExpression(
        projectionExpression.Type, 
        projectionExpression.ItemProjector, 
        tupleParameterBindings, 
        projectionExpression.ResultType);
      var translatedQuery = new TranslatedQuery(
        query.DataSource,
        (Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, bool, object>)
          query.UntypedMaterializer,
        tupleParameterBindings,
        EnumerableUtils<Parameter<Tuple>>.Empty);
      delayedSequence = new DelayedSequence<TElement>(context.Session, translatedQuery, parameterContext);
      context.Session.RegisterUserDefinedDelayedQuery(delayedSequence.Task);
      context.MaterializationContext.MaterializationQueue.Enqueue(MaterializeSelf);
    }
  }
}