// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Rse;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// LINQ query translation result.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public class TranslatedQuery<TResult> : TranslatedQuery
  {
    /// <summary>
    /// Materializer.
    /// </summary>
    public readonly Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> Materializer;

    /// <summary>
    /// Gets the tuple parameter bindings.
    /// </summary>
    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; private set; }

    /// <summary>
    /// Gets the tuple parameters.
    /// </summary>
    public List<Parameter<Tuple>> TupleParameters { get; private set; }

    /// <inheritdoc/>
    public override sealed Delegate UntypedMaterializer
    {
      get { return Materializer; }
    }

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public TResult Execute(ParameterContext parameterContext)
    {
      return Materializer(DataSource, TupleParameterBindings, parameterContext);
    }


    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    public TranslatedQuery(RecordSet dataSource, 
      Func<IEnumerable<Tuple>, 
      Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer)
      : this(dataSource, materializer, new Dictionary<Parameter<Tuple>, Tuple>(), EnumerableUtils<Parameter<Tuple>>.Empty)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    /// <param name="tupleParameters">The tuple parameters.</param>
    public TranslatedQuery(RecordSet dataSource, 
      Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer, 
      Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, 
      IEnumerable<Parameter<Tuple>> tupleParameters)
      : base(dataSource)
    {
      Materializer = materializer;
      TupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
      TupleParameters = tupleParameters.ToList();
    }
  }
}