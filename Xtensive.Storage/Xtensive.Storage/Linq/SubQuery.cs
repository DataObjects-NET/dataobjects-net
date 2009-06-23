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
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class SubQuery<TElement> : IQueryable<TElement>
  {
    private readonly ProjectionExpression projectionExpression;
    private readonly TranslatedQuery<IEnumerable<TElement>> translatedQuery;

    public IEnumerator<TElement> GetEnumerator()
    {
      return translatedQuery.Execute().GetEnumerator();
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

    public SubQuery(ProjectionExpression projectionExpression, TranslatedQuery translatedQuery, Parameter<Tuple> parameter, Tuple tuple)
    {
      this.projectionExpression = new ProjectionExpression(projectionExpression.Type, projectionExpression.ItemProjector, projectionExpression.TupleParameterBindings, projectionExpression.ResultType);
      var query = ((TranslatedQuery<IEnumerable<TElement>>) translatedQuery);
      this.translatedQuery = new TranslatedQuery<IEnumerable<TElement>>(translatedQuery.DataSource, query.Materializer, query.TupleParameterBindings, EnumerableUtils<Parameter<Tuple>>.Empty);

      // Gather Parameter<Tuple> values from current ParameterScope for future use. 
      parameter.Value = tuple;
      foreach (var tupleParameter in query.TupleParameters) {
        var value = tupleParameter.Value;
        this.translatedQuery.TupleParameterBindings[tupleParameter] = value;
        this.projectionExpression.TupleParameterBindings[tupleParameter] = value;
      }
    }
  }
}