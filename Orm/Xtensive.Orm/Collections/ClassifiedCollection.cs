// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xtensive.Collections
{
  /// <summary>
  /// A collection classifying all the items by their classes.
  /// </summary>
  /// <typeparam name="TClass">The type of the class.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [DebuggerDisplay("Count = {Count}, ClassCount = {ClassCount}")]
  public class ClassifiedCollection<TClass, TItem> :
    IClassifiedCollection<TClass, TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<TItem> set = new();
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<TItem> list = new();
    private readonly Dictionary<TClass, List<TItem>> classified = new();

    #region Properties: Count, ClassCount, IsSet, ...

    /// <inheritdoc/>
    public bool IsSet { get; }

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int Count => list.Count;

    /// <inheritdoc/>
    public int ClassCount => classified.Keys.Count;

    /// <inheritdoc/>
    public Func<TItem, TClass[]> Classifier { get; private set; }

    #endregion

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      if (!set.Add(item) && IsSet)
        return;
      list.Add(item);
      var classes = Classifier.Invoke(item);
      foreach (var @class in classes) {
        var classList = classified.GetValueOrDefault(@class);
        if (classList is null) {
          classList = new List<TItem>();
          classified.Add(@class, classList);
        }
        classList.Add(item);
      }
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<TItem> items)
    {
      foreach (var item in items)
        Add(item);
    }

    /// <inheritdoc/>
    public bool Remove(TItem item)
    {
      if (!set.Remove(item))
        return false;
      _ = list.Remove(item);
      var classes = Classifier.Invoke(item);
      foreach (var @class in classes) {
        var classList = classified[@class];
        _ = classList.Remove(item);
        if (classList.Count==0)
          _ = classified.Remove(@class);
      }
      return true;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      set.Clear();
      list.Clear();
      classified.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(TItem item) => set.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex)
      => list.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerable<TItem> GetItems(TClass @class)
      => classified.GetValueOrDefault(@class)
           ?? Enumerable.Empty<TItem>();

    /// <inheritdoc/>
    public IEnumerable<TClass> GetClasses() => classified.Keys;

    public int GetItemCount(TClass @class)
    {
      var items = classified.GetValueOrDefault(@class);
      return items != null ? items.Count : 0;
    }

    #region IEnumerable<...> members

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => list.GetEnumerator();

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="classifier">The classifier function.</param>
    public ClassifiedCollection(Func<TItem, TClass[]> classifier)
      : this(classifier, false)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="classifier">The classifier function.</param>
    /// <param name="isSet">Indicates whether this instance behaves like a set.</param>
    public ClassifiedCollection(Func<TItem, TClass[]> classifier, bool isSet)
    {
      Classifier = classifier;
      IsSet = isSet;
    }
  }
}