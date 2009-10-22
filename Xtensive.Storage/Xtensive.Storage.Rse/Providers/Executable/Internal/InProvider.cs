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

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public class InProvider : BinaryExecutableProvider<Compilable.InProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var rightEnumerable = Right.Enumerate(context);
      return Left
        .Enumerate(context)
        .Where(left => rightEnumerable.Contains(Origin.MapTransform.Apply(TupleTransformType.Auto, left)));
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public InProvider(Compilable.InProvider origin, ExecutableProvider source, ExecutableProvider filter)
      : base(origin, source, filter)
    {
    }
  }
}