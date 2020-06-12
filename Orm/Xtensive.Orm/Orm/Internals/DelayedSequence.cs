// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class DelayedSequence<T> : DelayedQueryResult, IEnumerable<T>
  {
    private readonly ParameterContext parameterContext;
    private readonly Materializer materializer;
    private readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;

    public IEnumerator<T> GetEnumerator() => Materialize(Session).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private SequenceQueryResult<T> Materialize(Session session)
    {
      if (!LifetimeToken.IsActive) {
        throw new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries);
      }

      if (Task.Result==null) {
        session.ExecuteUserDefinedDelayedQueries(false);
      }

      return materializer.Invoke<T>(TupleReader.Create(Task.Result), session, tupleParameterBindings, parameterContext);
    }

    // Constructors

    public DelayedSequence(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    {
      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      this.parameterContext = parameterContext;
    }
  }
}