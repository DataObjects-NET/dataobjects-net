// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.07

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// General index provider for all indexing storage handlers.
  /// </summary>
  internal sealed class IndexProvider : ExecutableProvider
  {
    private readonly Func<IndexInfo, IOrderedIndex<Tuple, Tuple>> indexResolver;
    private readonly IndexInfo indexDescriptor;

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      var index = indexResolver(indexDescriptor);
      var result = index as T;
      return result;
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var index = indexResolver(indexDescriptor);
      return index;
    }


    // Constructors

    public IndexProvider(CompilableProvider origin, IndexInfo indexDescriptor, Func<IndexInfo,IOrderedIndex<Tuple,Tuple>> indexResolver)
      : base(origin)
    {
      this.indexResolver = indexResolver;
      this.indexDescriptor = indexDescriptor;
    }
  }
}