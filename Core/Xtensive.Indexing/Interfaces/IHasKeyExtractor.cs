// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;

namespace Xtensive.Indexing
{
  /// <summary>
  /// An object having <see cref="KeyExtractor"/> contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface IHasKeyExtractor<TKey, TItem>
  {
    /// <summary>
    /// Gets key extractor associated with the index.
    /// </summary>
    Converter<TItem, TKey> KeyExtractor { get; }
  }
}