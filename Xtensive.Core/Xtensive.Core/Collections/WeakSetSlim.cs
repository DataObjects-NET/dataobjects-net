// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.07

using System;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Set of weak references.
  /// </summary>
  /// <typeparam name="TItem">Type of items to be stored in set.</typeparam>
  public class WeakSetSlim<TItem> : SetBase<TItem> 
    where TItem : class
  {
    private readonly WeakestDictionary<TItem, TItem> dictionary;

    /// <inheritdoc/>
    protected override IDictionary<TItem, TItem> Items
    {
      get { return dictionary; }
    }

    /// <summary>
    /// Removes dead references from set.
    /// </summary>
    public void Cleanup()
    {
      dictionary.Cleanup();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public WeakSetSlim()
      : this((IEqualityComparer<TItem>)null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Equality comparer for the set type.</param>
    public WeakSetSlim(IEqualityComparer<TItem> comparer) 
      : base(comparer)
    {
      dictionary = new WeakestDictionary<TItem, TItem>(comparer);
      ContainsNull = false;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    public WeakSetSlim(IEnumerable<TItem> items) 
      : base(items is SetBase<TItem> ? (items as SetBase<TItem>).Comparer : EqualityComparer<TItem>.Default)
    {
      ICollection<TItem> collection = items as ICollection<TItem>;
      dictionary = collection!=null
        ? new WeakestDictionary<TItem, TItem>(collection.Count, Comparer)
        : new WeakestDictionary<TItem, TItem>(Comparer);
      ContainsNull = false;
      this.UnionWith(items);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    /// <param name="comparer">Equality comparer for the set type.</param>
    public WeakSetSlim(IEnumerable<TItem> items, IEqualityComparer<TItem> comparer)
      : this(comparer)
    {
      this.UnionWith(items);
    }
  }
}