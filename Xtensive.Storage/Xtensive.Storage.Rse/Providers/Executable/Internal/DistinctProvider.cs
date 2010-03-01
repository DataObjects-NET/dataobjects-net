// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class DistinctProvider : UnaryExecutableProvider<Compilable.DistinctProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      HashSet<Tuple> x = new HashSet<Tuple>();
      foreach (var item in Source.Enumerate(context)) {
        if (!x.Add(item))
          continue;
        yield return item;
      }
    }


    // Constructors

    public DistinctProvider(Compilable.DistinctProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}