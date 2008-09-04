// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class FilterProvider : UnaryExecutableProvider<Compilable.FilterProvider>
  {
    private Func<Tuple, bool> predicate;

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Where(predicate);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      predicate = Origin.Predicate.Compile();
    }


    // Constructors

    public FilterProvider(Compilable.FilterProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}