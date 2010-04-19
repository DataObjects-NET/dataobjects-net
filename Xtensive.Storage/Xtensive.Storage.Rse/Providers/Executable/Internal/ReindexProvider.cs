// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ReindexProvider : UnaryExecutableProvider<Compilable.ReindexProvider>
  {
    private const string IndexKey = "IndexKey";
    private IndexConfiguration<Tuple, Tuple> indexConfiguration;

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var index = new Index<Tuple, Tuple>(indexConfiguration);
      foreach (Tuple tuple in Source.Enumerate(context))
        index.Add(tuple);
      SetCachedValue(context, IndexKey, index);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return GetCachedValue<Index<Tuple, Tuple>>(context, IndexKey);
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      if (typeof(T) == typeof(ICachingProvider))
        return base.GetService<T>();
      return GetCachedValue<Index<Tuple, Tuple>>(EnumerationScope.CurrentContext, IndexKey) as T;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      indexConfiguration = new IndexConfiguration<Tuple, Tuple> {
        KeyComparer = Origin.OrderKeyComparer, 
        KeyExtractor = Origin.OrderKeyExtractor
      };
      var index = new Index<Tuple, Tuple>(indexConfiguration);
      SetCachedValue(EnumerationContext.Current, IndexKey, index);
    }


    // Constructors

    public ReindexProvider(Compilable.ReindexProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<ICachingProvider>();
    }
  }
}