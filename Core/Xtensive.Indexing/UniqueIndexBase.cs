// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.15

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base class for all unique indexes.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public abstract class UniqueIndexBase<TKey, TItem>: IndexBase<TKey, TItem>,
    IUniqueIndex<TKey, TItem>
  {
    /// <inheritdoc/>
    public TItem Resolve(TKey identifier)
    {
      return GetItem(identifier);
    }

    /// <inheritdoc/>
    public abstract TItem GetItem(TKey key);

    
    // Constructors

    /// <inheritdoc/>
    protected UniqueIndexBase()
    {
    }

    /// <inheritdoc/>
    protected UniqueIndexBase(IndexConfigurationBase<TKey, TItem> configuration)
      : base(configuration)
    {
    }
  }
}