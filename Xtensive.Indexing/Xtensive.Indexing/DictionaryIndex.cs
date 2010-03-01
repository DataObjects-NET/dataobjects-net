// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.18

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Unique index based on <see cref="Dictionary{TKey,TValue}"/>.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public sealed class DictionaryIndex<TKey, TItem>: UniqueIndexBase<TKey, TItem>
  {
    private readonly Dictionary<TKey, TItem> dictionary = new Dictionary<TKey, TItem>();

    /// <inheritdoc/>
    public override long Count
    {
      [DebuggerStepThrough]
      get { return dictionary.Count; }
    }

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      return dictionary[key];
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return dictionary.ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return dictionary.ContainsKey(key);
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public override void Add(TItem item)
    {
      dictionary.Add(KeyExtractor(item), item);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      return dictionary.Remove(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      dictionary[KeyExtractor(item)] = item;
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      return dictionary.Remove(key);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      dictionary.Clear();
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public override IEnumerator<TItem> GetEnumerator()
    {
      return dictionary.Values.GetEnumerator();
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}