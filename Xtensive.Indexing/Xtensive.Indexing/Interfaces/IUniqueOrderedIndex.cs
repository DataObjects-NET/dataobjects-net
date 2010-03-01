// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.14

namespace Xtensive.Indexing
{
  /// <summary>
  /// Unique ordered index contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface IUniqueOrderedIndex<TKey,TItem> : 
    IUniqueIndex<TKey, TItem>, 
    IOrderedIndex<TKey, TItem>
  {
  }
}