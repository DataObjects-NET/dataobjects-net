// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ReindexProvider : UnaryExecutableProvider<Compilable.ReindexProvider>
  {
    private IndexConfiguration<Tuple, Tuple> indexConfiguration;

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var index = new Index<Tuple, Tuple>(indexConfiguration);
      foreach (Tuple tuple in Source.Enumerate(context))
        index.Add(tuple);
      return index;
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      if (typeof(T) == typeof(ICachingProvider))
        return base.GetService<T>();
      var context = EnumerationScope.CurrentContext;
      if (context == null)
        return new Index<Tuple, Tuple>(indexConfiguration) as T;
      return Enumerate(context) as T;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      indexConfiguration = new IndexConfiguration<Tuple, Tuple> {
        KeyComparer = Origin.OrderKeyComparer, 
        KeyExtractor = Origin.OrderKeyExtractor
      };
    }


    // Constructors

    public ReindexProvider(Compilable.ReindexProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<ICachingProvider>();
    }
  }
}