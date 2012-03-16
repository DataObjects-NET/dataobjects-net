// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// A collection of <see cref="HierarchyDef"/> items.
  /// </summary>
  public class HierarchyDefCollection : CollectionBase<HierarchyDef>
  {
    /// <inheritdoc/>
    public override bool Contains(HierarchyDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return TryGetValue(item.Root) != null;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(TypeDef key)
    {
      return TryGetValue(key.UnderlyingType);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(Type key)
    {
      foreach (HierarchyDef item in this)
        if (item.Root.UnderlyingType==key)
          return item;
      return null;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public HierarchyDef this[Type key]
    {
      get
      {
        HierarchyDef result = TryGetValue(key);
        if (result!=null)
          return result;
          throw new ArgumentException(String.Format(Strings.ExItemByKeyXWasNotFound, key), "key");
      }
    }
  }
}