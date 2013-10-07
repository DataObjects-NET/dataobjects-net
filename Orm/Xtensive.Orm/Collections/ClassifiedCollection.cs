// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// A collection classifying all the items by their classes.
  /// </summary>
  /// <typeparam name="TClass">The type of the class.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}, ClassCount = {ClassCount}")]
  public class ClassifiedCollection<TClass, TItem> : 
    IClassifiedCollection<TClass, TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private HashSet<TItem> set = new HashSet<TItem>();
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private List<TItem> list = new List<TItem>();
    private Dictionary<TClass, List<TItem>> classified = new Dictionary<TClass, List<TItem>>();

    #region Properties: Count, ClassCount, IsSet, ...

    /// <inheritdoc/>
    public bool IsSet { get; private set; }

    /// <inheritdoc/>
    public bool IsReadOnly {
      get { return false; }
    }

    /// <inheritdoc/>
    public int Count {
      get { return list.Count; }
    }

    /// <inheritdoc/>
    public int ClassCount {
      get { return classified.Keys.Count; }
    }

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
        if (classList==null) {
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
      list.Remove(item);
      var classes = Classifier.Invoke(item);
      foreach (var @class in classes) {
        var classList = classified[@class];
        classList.Remove(item);
        if (classList.Count==0)
          classified.Remove(@class);
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
    public bool Contains(TItem item)
    {
      return set.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      list.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerable<TItem> GetItems(TClass @class)
    {
      return classified.GetValueOrDefault(@class) ?? EnumerableUtils<TItem>.Empty;
    }

    /// <inheritdoc/>
    public IEnumerable<TClass> GetClasses()
    {
      return classified.Keys;
    }

    #region IEnumerable<...> members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<TItem> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="classifier">The classifier function.</param>
    public ClassifiedCollection(Func<TItem, TClass[]> classifier)
      : this(classifier, false)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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