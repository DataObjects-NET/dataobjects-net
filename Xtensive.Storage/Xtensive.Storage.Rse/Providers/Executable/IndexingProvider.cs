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
  public sealed class IndexingProvider : UnaryExecutableProvider
  {
    private IndexConfiguration<Tuple, Tuple> indexConfiguration;

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
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
      return Enumerate(EnumerationScope.CurrentContext) as T;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var origin = (Compilable.IndexingProvider) Origin;
      indexConfiguration = new IndexConfiguration<Tuple, Tuple> {
        KeyComparer = origin.OrderKeyComparer, 
        KeyExtractor = origin.OrderKeyExtractor
      };
    }


    // Constructors

    public IndexingProvider(CompilableProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<ICachingProvider>();
    }
  }
}