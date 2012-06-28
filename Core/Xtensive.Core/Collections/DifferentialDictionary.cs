// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Collections
{
  /// <summary>
  /// The dictionary storing its content as difference to its <see cref="Origin"/>.
  /// <see cref="Origin"/> must not be modified during 
  /// <see cref="DifferentialDictionary{TKey,TValue}"/> lifetime (usage period).
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}, Changes = {Changes}")]
  public sealed class DifferentialDictionary<TKey, TValue> : IDictionary<TKey, TValue>
  {
    private readonly static Dictionary<TKey, TValue> emptyDictionary = new Dictionary<TKey, TValue>();
    private static readonly HashSet<TKey> emptyHashSet = new HashSet<TKey>();

    private readonly IDictionary<TKey, TValue> origin;
    private Dictionary<TKey, TValue> addedItems;
    private Dictionary<TKey, TValue> removedItems;
    private HashSet<TKey> changedItems; // Contains keys of added, removed or changed items
    [NonSerialized]
    private int version;

    /// <summary>
    /// Gets the origin.
    /// </summary>
    public IDictionary<TKey, TValue> Origin
    {
      get { return origin; }
    }

    /// <summary>
    /// Gets the difference of the current state with <see cref="Origin"/>.
    /// This method requires constant time 
    /// </summary>
    public DifferentialDictionaryDifference<TKey, TValue> Difference {
      get {
        return new DifferentialDictionaryDifference<TKey, TValue>(
          new ReadOnlyDictionary<TKey, TValue>(GetReadOnlyAddedItems()),
          new ReadOnlyDictionary<TKey, TValue>(GetReadOnlyRemovedItems()),
          new ReadOnlyHashSet<TKey>(GetReadOnlyChangedItems()));
      }
    }

    /// <summary>
    /// Commits the changes (<see cref="Difference"/>) to the <see cref="Origin"/> dictionary.
    /// </summary>
    public void ApplyChanges()
    {
      foreach (var item in GetReadOnlyRemovedItems())
        origin.Remove(item.Key);
      foreach (var item in GetReadOnlyAddedItems())
        origin.Add(item.Key, item.Value);
      CancelChanges();
    }

    /// <summary>
    /// Cancels (cleans up) all the changes (<see cref="Difference"/>).
    /// </summary>
    public void CancelChanges()
    {
      unchecked { version++; }
      addedItems = null;
      removedItems = null;
      changedItems = null;
    }

    /// <inheritdoc/>
    public bool IsReadOnly {
      get { return false; }
    }

    /// <inheritdoc/>
    public int Count {
      get {
        return GetReadOnlyAddedItems().Count - GetReadOnlyRemovedItems().Count + origin.Count;
      }
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value)
    {
      if (GetReadOnlyAddedItems().TryGetValue(key, out value))
        return true;
      if (GetReadOnlyRemovedItems().ContainsKey(key)) {
        value = default(TValue);
        return false;
      }
      return origin.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the <typeparamref name="TValue"/> with the specified key.
    /// </summary>
    public TValue this[TKey key] {
      get {
        TValue value;
        if (TryGetValue(key, out value))
          return value;
        throw new KeyNotFoundException(string.Format(
          Strings.ExKeyXIsNotFound, key));
      }
      set {
        if (ContainsKey(key))
          Remove(key);
        Add(key, value);
      }
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      if (GetReadOnlyAddedItems().ContainsKey(key))
        return true;
      if (GetReadOnlyRemovedItems().ContainsKey(key))
        return false;
      return origin.ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return ContainsKey(item.Key);
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
      if (GetReadOnlyAddedItems().ContainsKey(key))
        throw new ArgumentException(Strings.ExKeyAlreadyExists, "key");
      if (GetReadOnlyRemovedItems().ContainsKey(key)) {
        // Item was removed before
        unchecked { version++; }
        GetOrCreateAddedItems().Add(key, value);
        return;
      }
      if (origin.ContainsKey(key))
        throw new ArgumentException(Strings.ExKeyAlreadyExists, "key");
      unchecked { version++; }
      GetOrCreateAddedItems().Add(key, value);
      GetOrCreateChangedItems().Add(key);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
      var added = GetReadOnlyAddedItems();
      if (added.ContainsKey(key)) {
        unchecked { version++; }
        added.Remove(key);
        return true;
      }
      if (GetReadOnlyRemovedItems().ContainsKey(key)) {
        return false;
      }
      TValue value;
      if (!origin.TryGetValue(key, out value))
        return false;
      unchecked { version++; }
      GetOrCreateRemovedItems().Add(key, value);
      GetOrCreateChangedItems().Add(key);
      return true;
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      addedItems = null;
      removedItems = new Dictionary<TKey, TValue>();
      GetOrCreateChangedItems(); // We must keep previously made changes!
      foreach (var pair in origin) {
        removedItems.Add(pair.Key, pair.Value);
        changedItems.Add(pair.Key);
      }
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      this.ToList().CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      int originalVersion = version;
      foreach (var pair in GetReadOnlyAddedItems()) {
        if (version!=originalVersion)
          throw new InvalidOperationException(Strings.ExCollectionHasBeenModified);
        yield return pair;
      }
      var removed = GetReadOnlyRemovedItems();
      foreach (var pair in origin) {
        if (removed.ContainsKey(pair.Key))
          continue;
        if (version!=originalVersion)
          throw new InvalidOperationException(Strings.ExCollectionHasBeenModified);
        yield return pair;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// <inheritdoc/>
    /// This property requires <see langword="O(Count)" /> time to return the result.
    /// </summary>
    public ICollection<TKey> Keys {
      get {
        return 
          GetReadOnlyAddedItems().Keys
          .Concat(origin.Keys.Except(GetReadOnlyRemovedItems().Keys))
          .ToList(); 
      }
    }

    /// <summary>
    /// <inheritdoc/>
    /// This property requires <see langword="O(Count)" /> time to return the result.
    /// </summary>
    public ICollection<TValue> Values {
      get {
        var removed = GetReadOnlyRemovedItems();
        return 
          GetReadOnlyAddedItems().Values
          .Concat(origin
            .Where(pair => !removed.ContainsKey(pair.Key))
            .Select(pair => pair.Value))
          .ToList();
      }
    }

    #region Private methods

    private Dictionary<TKey, TValue> GetOrCreateAddedItems()
    {
      if (addedItems==null)
        addedItems = new Dictionary<TKey, TValue>();
      return addedItems;
    }

    private Dictionary<TKey, TValue> GetOrCreateRemovedItems()
    {
      if (removedItems==null)
        removedItems = new Dictionary<TKey, TValue>();
      return removedItems;
    }

    private HashSet<TKey> GetOrCreateChangedItems()
    {
      if (changedItems==null)
        changedItems = new HashSet<TKey>();
      return changedItems;
    }

    private Dictionary<TKey, TValue> GetReadOnlyAddedItems()
    {
      return addedItems ?? emptyDictionary;
    }

    private Dictionary<TKey, TValue> GetReadOnlyRemovedItems()
    {
      return removedItems ?? emptyDictionary;
    }

    private HashSet<TKey> GetReadOnlyChangedItems()
    {
      return changedItems ?? emptyHashSet;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The original dictionary.</param>
    public DifferentialDictionary(IDictionary<TKey, TValue> origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");
      this.origin = origin;
    }
  }
}