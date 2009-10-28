// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public class IncludeProvider : UnaryExecutableProvider<Compilable.IncludeProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sources = Source.Enumerate(context);
      var tuples = Origin
        .Tuples
        .CachingCompile()
        .Invoke();
      foreach (var source in sources) {
        var checkTuple = Origin.FilterTransform.Apply(TupleTransformType.Auto, source);
        bool isContains = tuples.Contains(checkTuple);
        var newTuple = Origin.CombineTransform.Apply(TupleTransformType.Auto, source, Tuple.Create(isContains));
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