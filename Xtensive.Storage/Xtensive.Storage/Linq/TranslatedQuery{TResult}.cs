// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using System.Collections.Generic;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class TranslatedQuery<TResult> : TranslatedQuery
  {
    public readonly Func<RecordSet, IEnumerable<Pair<Parameter<Tuple>, Tuple>>, TResult> Materializer;
    public IEnumerable<Pair<Parameter<Tuple>, Tuple>> TupleParameterBindings { get; private set; }
    
    public sealed override Delegate UntypedMaterializer
    {
      get { return Materializer; }
    }

    public TResult Execute()
    {
      return Materializer(DataSource, TupleParameterBindings);
    }


    // Constructors

    public TranslatedQuery(RecordSet dataSource, Func<RecordSet, IEnumerable<Pair<Parameter<Tuple>, Tuple>>, TResult> materializer)
      : this(dataSource, materializer, ArrayUtils<Pair<Parameter<Tuple>, Tuple>>.EmptyArray)
    {
    }

    public TranslatedQuery(RecordSet dataSource, Func<RecordSet, IEnumerable<Pair<Parameter<Tuple>, Tuple>>, TResult> materializer, IEnumerable<Pair<Parameter<Tuple>, Tuple>> tupleParameterBindings)
      :base (dataSource)
    {
      Materializer = materializer;
      TupleParameterBindings = tupleParameterBindings;
    }
  }
}