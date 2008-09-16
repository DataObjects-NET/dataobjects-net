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
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Index
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

    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var index = indexResolver(indexDescriptor);
      return index;
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="indexDescriptor">Descriptor of the index.</param>
    /// <param name="indexResolver">Index resolver function.</param>
    public IndexProvider(CompilableProvider origin, IndexInfoRef indexDescriptor, Func<IndexInfoRef,IOrderedIndex<Tuple,Tuple>> indexResolver)
      : base(origin)
    {
      this.indexResolver = indexResolver;
      this.indexDescriptor = indexDescriptor;
    }
  }
}