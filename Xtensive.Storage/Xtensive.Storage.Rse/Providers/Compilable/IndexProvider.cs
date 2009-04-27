// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Gives access to the specified <see cref="Index"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : LocationAwareProvider
  {
    private static ThreadSafeDictionary<IndexInfo, IndexProvider> cache = 
      ThreadSafeDictionary<IndexInfo, IndexProvider>.Create(new object());
    private readonly RecordSetHeader indexHeader;

    /// <summary>
    /// Reference to the <see cref="IndexInfo"/> instance within the domain.
    /// </summary>
    public IndexInfoRef Index { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Index.ToString();
    }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return indexHeader.Order;
    }


    // Factory method

    /// <summary>
    /// Gets the <see cref="IndexProvider"/> for the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to get the provider for.</param>
    /// <returns>Existing or newly created provider for the specified <paramref name="index"/>.</returns>
    public static IndexProvider Get(IndexInfo index)
    {
      return cache.GetValue(index, _index => new IndexProvider(_index));
    }


    // Constructor

    private IndexProvider(IndexInfo index)
      : base (ProviderType.Index, RseCompiler.DefaultServerLocation)
    {
      indexHeader = index.GetRecordSetHeader();
      Index = new IndexInfoRef(index);
      Initialize(typeof(IndexProvider)); // Since .ctor is private!
    }
  }
}