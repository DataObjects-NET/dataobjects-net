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
using System.Linq;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  public class TranslatedQuery<TResult> : TranslatedQuery
  {
    public readonly Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult> Materializer;
    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; private set; }
    public IEnumerable<Parameter<Tuple>> TupleParameters { get; private set; }

    public override sealed Delegate UntypedMaterializer
    {
      get { return Materializer; }
    }

    public TResult Execute()
    {
      return Materializer(DataSource, TupleParameterBindings);
    }


    // Constructors

    public TranslatedQuery(IEnumerable<Tuple> dataSource, Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult> materializer)
      : this(dataSource, materializer, new Dictionary<Parameter<Tuple>, Tuple>(), EnumerableUtils<Parameter<Tuple>>.Empty)
    {
    }

    public TranslatedQuery(IEnumerable<Tuple> dataSource, Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult> materializer, Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, IEnumerable<Parameter<Tuple>> tupleParameters)
      : base(dataSource)
    {
      Materializer = materializer;
      TupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
      TupleParameters = tupleParameters.ToList();
    }
  }
}