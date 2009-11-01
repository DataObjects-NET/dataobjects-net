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

namespace Xtensive.Storage.Rse.Providers.Implementation
{
  /// <summary>
  /// General index provider for all indexing storage handlers.
  /// </summary>
  public sealed class IndexProvider : ProviderImplementation
  {
    private readonly Func<IndexInfo, IOrderedIndex<Tuple, Tuple>> indexResolver;
    private readonly IndexInfo indexDescriptor;

    /// <inheritdoc/>
    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Indexed | ProviderOptions.Ordered | ProviderOptions.FastFirst | ProviderOptions.FastCount; }
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      var index = indexResolver(indexDescriptor);
      var result = index as T;
      return result;
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      var index = indexResolver(indexDescriptor);
      return index.GetEnumerator();
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="indexResolver">Delegate that returns <see cref="IOrderedIndex{TKey,TItem}"/> by provided <see cref="IndexInfo"/>.</param>
    /// <param name="indexDescriptor">Descriptor of the index.</param>
    /// <param name="header">Result header.</param>
    public IndexProvider(RecordHeader header, IndexInfo indexDescriptor, Func<IndexInfo,IOrderedIndex<Tuple,Tuple>> indexResolver)
      : base(header)
    {
      this.indexResolver = indexResolver;
      this.indexDescriptor = indexDescriptor;
    }
  }
}