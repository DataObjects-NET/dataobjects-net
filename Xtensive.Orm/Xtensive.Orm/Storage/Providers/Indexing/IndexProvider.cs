// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.07

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Rse.Providers;
using StorageIndexInfo = Xtensive.Storage.Model.IndexInfo;

namespace Xtensive.Storage.Providers.Indexing
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
      var storageContext = (EnumerationContext)EnumerationContext.Current;
      var index = indexResolver.GetIndex(indexDescriptor, storageContext.SessionHandler);
      var result = index as T;
      return result;
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var storageContext = (EnumerationContext) context;
      var index = indexResolver.GetIndex(indexDescriptor, storageContext.SessionHandler);
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