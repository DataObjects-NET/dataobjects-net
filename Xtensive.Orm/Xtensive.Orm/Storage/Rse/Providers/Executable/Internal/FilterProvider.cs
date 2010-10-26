// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class FilterProvider : UnaryExecutableProvider<Compilable.FilterProvider>
  {
    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Where(Origin.CompiledPredicate);
    }


    // Constructors

    public FilterProvider(Compilable.FilterProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}