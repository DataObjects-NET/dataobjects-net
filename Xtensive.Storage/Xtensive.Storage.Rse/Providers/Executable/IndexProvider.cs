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

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// General index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : ExecutableProvider
  {
    private readonly Func<IndexInfoRef, IOrderedIndex<Tuple, Tuple>> indexResolver;
    private readonly IndexInfoRef indexDescriptor;

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      var index = indexResolver(indexDescriptor);
      var result = index as T;
      return result;
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var index = indexResolver(indexDescriptor);
      return index;
    }


    // Constructors

    public IndexProvider(CompilableProvider origin, IndexInfoRef indexDescriptor, Func<IndexInfoRef,IOrderedIndex<Tuple,Tuple>> indexResolver)
      : base(origin)
    {
      this.indexResolver = indexResolver;
      this.indexDescriptor = indexDescriptor;
    }
  }
}