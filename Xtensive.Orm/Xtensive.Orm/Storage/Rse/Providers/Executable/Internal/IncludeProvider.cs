// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Executable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if source value equal to one of provided values; 
  /// otherwise, <see langword="false" />.
  /// </summary>
  [Serializable]
  public class IncludeProvider : UnaryExecutableProvider<Compilable.IncludeProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sources = Source.Enumerate(context);
      var tuples = Origin.FilterDataSource.CachingCompile().Invoke().ToHashSet();
      foreach (var source in sources) {
        var checkTuple = Origin.FilteredColumnsExtractionTransform.Apply(TupleTransformType.Auto, source);
        bool isContains = tuples.Contains(checkTuple);
        var newTuple = Origin.ResultTransform.Apply(TupleTransformType.Auto, source, Tuple.Create(isContains));
        yield return newTuple;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IncludeProvider(Compilable.IncludeProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}