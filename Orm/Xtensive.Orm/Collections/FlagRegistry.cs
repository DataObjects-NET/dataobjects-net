// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.06

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Flag registry. An item could be registered multiple times with the different flags. 
  /// Flags usually is an <see cref="Enum"/> descendant.
  /// </summary>
  /// <typeparam name="TFlag">The type of the flag.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class FlagRegistry<TFlag, TItem>
  {
    private readonly Dictionary<TFlag, HashSet<TItem>> containers = new Dictionary<TFlag, HashSet<TItem>>();
    private readonly Func<TItem, TFlag> flagExtractor;

    /// <summary>
    /// Gets the total count of items registered in this instance.
    /// </summary>
    public int Count
    {
      get
      {
        if (containers.Count==0)
          return 0;

        int result = 0;
        foreach (HashSet<TItem> items in containers.Values)
          result += items.Count;
        return result;
      }
    }

    /// <summary>
    /// Registers the specified item in this instance.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Register(TItem item)
    {
      GetContainer(flagExtractor(item)).Add(item);
    }

    /// <summary>
    /// Gets the flags.
    /// </summary>
    public IEnumerable<TFlag> GetFlags()
    {
      return containers.Keys;
    }

    /// <summary>
    /// Gets the items by specified flag.
    /// </summary>
    /// <param name="flag">The flag.</param>
    public HashSet<TItem> GetItems(TFlag flag)
    {
      return GetContainer(flag);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
      containers.Clear();
    }

    private HashSet<TItem> GetContainer(TFlag flag)
    {
      HashSet<TItem> container;
      if (!containers.TryGetValue(flag, out container)) {
        container = new HashSet<TItem>();
        containers.Add(flag, container);
      }
      return container;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      if (Count == 0)
        return "Count: 0";
      return containers.Select(p => string.Format("{0}: {1}", p.Key, p.Value.Count)).ToCommaDelimitedString();
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="flagExtractor">The flag extractor.</param>
    public FlagRegistry(Func<TItem, TFlag> flagExtractor)
    {
      this.flagExtractor = flagExtractor;
    }
  }
}