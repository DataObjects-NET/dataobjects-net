// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.07

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Rse.Providers;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// General index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : ExecutableProvider
  {
    private readonly IIndexResolver indexResolver;
    private readonly StorageIndexInfo indexDescriptor;

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      var index = indexResolver.GetIndex(indexDescriptor);
      var result = index as T;
      return result;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var index = indexResolver.GetIndex(indexDescriptor);
      return index;
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="indexDescriptor">Descriptor of the index.</param>
    /// <param name="indexResolver">Index resolver.</param>
    public IndexProvider(CompilableProvider origin,
      StorageIndexInfo indexDescriptor, IIndexResolver indexResolver)
      : base(origin)
    {
      this.indexResolver = indexResolver;
      this.indexDescriptor = indexDescriptor;
    }
  }
}