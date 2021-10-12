// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class SubQuery<TElement> :
    IOrderedQueryable<TElement>,
    IOrderedEnumerable<TElement>
  {
    private readonly ProjectionExpression projectionExpression;
    private DelayedQuery<TElement> delayedQuery;
    private List<TElement> materializedSequence;
    private readonly QueryProvider provider;

    public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
      throw new NotSupportedException();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      if (materializedSequence == null)
        materializedSequence = delayedQuery.ToList();
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
      materializedSequence = delayedQuery.ToList();
      delayedQuery = null;
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
        projectionExpression.ResultAccessMethod);
      var translatedQuery = new TranslatedQuery(
        query.DataSource,
        query.Materializer,
        query.ResultAccessMethod,
        tupleParameterBindings,
        Array.Empty<Parameter<Tuple>>());
      delayedQuery = new DelayedQuery<TElement>(context.Session, translatedQuery, parameterContext);
      context.Session.RegisterUserDefinedDelayedQuery(delayedQuery.Task);
      context.MaterializationContext.MaterializationQueue.Enqueue(MaterializeSelf);
    }
  }
}
