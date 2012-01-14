// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.21

using System;
using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// A collection of <see cref="FullTextIndexDef"/> items.
  /// </summary>
  [Serializable]
  public class FullTextIndexDefCollection : CollectionBase<FullTextIndexDef>
  {
    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public FullTextIndexDef TryGetValue(TypeDef key)
    {
      return TryGetValue(key.UnderlyingType);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public FullTextIndexDef TryGetValue(Type key)
    {
      foreach (var item in this)
        if (item.Type.UnderlyingType == key)
          return item;
      return null;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public FullTextIndexDef this[Type key]
    {
      get
      {
        var result = TryGetValue(key);
        if (result != null)
          return result;
        throw new ArgumentException(String.Format(Resources.Strings.ExItemByKeyXWasNotFound, key), "key");
      }
    }
  }
}