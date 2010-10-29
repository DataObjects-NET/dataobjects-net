// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.24

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Represents a set of items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class SetSlim<TItem> : SetBase<TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly Dictionary<TItem, TItem> dictionary;

    /// <inheritdoc/>
    protected override IDictionary<TItem, TItem> Items
    {
      get { return dictionary; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SetSlim()
      : this((IEqualityComparer<TItem>)null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="comparer">The equality comparer to use.</param>
    public SetSlim(IEqualityComparer<TItem> comparer)
      : base(comparer)
    {
      dictionary = new Dictionary<TItem, TItem>(Comparer);
      ContainsNull = false;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    public SetSlim(IEnumerable<TItem> items)
      : base(items is SetBase<TItem> ? (items as SetBase<TItem>).Comparer : null)
    {
      ICollection<TItem> collection = items as ICollection<TItem>;
      dictionary = collection!=null
        ? new Dictionary<TItem, TItem>(collection.Count, Comparer)
        : new Dictionary<TItem, TItem>(Comparer);
      ContainsNull = false;
      this.UnionWith(items);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    /// <param name="comparer">The equality comparer to use.</param>
    public SetSlim(IEnumerable<TItem> items, IEqualityComparer<TItem> comparer)
      : this(comparer)
    {
      this.UnionWith(items);
    }
  }
}