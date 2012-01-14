// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.15

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Notifications;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Optimization;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Universal generic index to be used with <see cref="ICollectionChangeNotifier{TItem}"/> implementors and 
  /// optionally with <see cref="IChangeNotifier"/> implementors.
  /// </summary>
  /// <remarks>This type is not intended to be used directly. It throws <see cref="NotSupportedException"/> on 
  /// <see cref="Add(TItem)"/>, <see cref="Remove(TItem)"/>, <see cref="Clear"/> operations.</remarks>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public sealed class CollectionIndex<TKey, TItem>: CollectionIndexBase,
    IUniqueIndex<TKey, TItem>,
    INonUniqueIndex<TKey, TItem>
  {
    private readonly string name;
    private readonly IIndex<TKey, TItem> index;
    private readonly IUniqueIndex<TKey, TItem> uniqueIndex;
    private readonly INonUniqueIndex<TKey, TItem> nonUniqueIndex;
    private readonly IOrderedIndex<TKey, TItem> orderedIndex;
    private readonly IMeasurable<TItem> measurable;
    private readonly IRangeMeasurable<TKey, TItem> rangeMeasurable;
    private readonly Dictionary<TItem, TKey> pendingItems = new Dictionary<TItem, TKey>();

    #region Properties: Name, Index

    /// <inheritdoc/>
    public override string Name
    {
      [DebuggerStepThrough]
      get { return name; }
    }

    /// <inheritdoc/>
    protected override IIndex Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    #endregion

    #region Properties: comparers, key exptractor

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get { return index.KeyExtractor; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<TKey> KeyComparer
    {
      [DebuggerStepThrough]
      get { return orderedIndex.KeyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<TKey>> EntireKeyComparer
    {
      [DebuggerStepThrough]
      get { return orderedIndex.EntireKeyComparer; }
    }

    /// <inheritdoc/>
    public Func<Entire<TKey>, TKey, int> AsymmetricKeyCompare
    {
      [DebuggerStepThrough]
      get { return orderedIndex.AsymmetricKeyCompare; }
    }

    #endregion
    
    #region IMeasurable<TItem> Members

    public long Size
    {
      [DebuggerStepThrough]
      get { return (long)GetMeasureResult(SizeMeasure<object>.CommonName); }
    }

    public bool HasMeasures
    {
      [DebuggerStepThrough]
      get
      {
        if (measurable == null)
          return false;
        return measurable.Measures.Count != 0;
      }
    }

    /// <inheritdoc/>
    public IMeasureSet<TItem> Measures {
      [DebuggerStepThrough]
      get {
        if (measurable == null)
          throw new NotSupportedException();
        return measurable.Measures;
      }
    }

    /// <inheritdoc/>
    public object GetMeasureResult(string name)
    {
      if (measurable == null)
        throw new NotSupportedException();
      return measurable.GetMeasureResult(name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(params string[] names)
    {
      if (measurable == null)
        throw new NotSupportedException();
      return measurable.GetMeasureResults(names);
    }

    #endregion

    #region IRangeMeasurable<TKey,TItem> Members

    /// <inheritdoc/>
    public object GetMeasureResult(Range<Entire<TKey>> range, string name)
    {
      if (rangeMeasurable == null)
        throw new NotSupportedException();
      return rangeMeasurable.GetMeasureResult(range, name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(Range<Entire<TKey>> range, params string[] names)
    {
      if (rangeMeasurable == null)
        throw new NotSupportedException();
      return rangeMeasurable.GetMeasureResults(range, names);
    }

    #endregion

    #region INonUniqueIndex<TKey,TItem> Members

    /// <inheritdoc/>
    IEnumerable<TItem> INonUniqueIndex<TKey, TItem>.GetItems(TKey key)
    {
      if (nonUniqueIndex == null)
        throw new NotSupportedException();
      return GetItems(key);
    }

    #endregion

    #region IUniqueIndex<TKey, TItem> Members

    TItem IUniqueIndex<TKey, TItem>.GetItem(TKey key)
    {
      return GetItem(key);
    }

    #endregion

    #region IIndex<TKey,TItem> Members

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always.</exception>
    void IIndex<TKey, TItem>.Add(TItem item)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always.</exception>
    bool IIndex<TKey, TItem>.Remove(TItem item)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always.</exception>
    void IIndex<TKey, TItem>.Replace(TItem item)
    {
      throw Exceptions.CollectionIsReadOnly("item");
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always.</exception>
    bool IIndex<TKey, TItem>.RemoveKey(TKey key)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    bool IIndex<TKey, TItem>.Contains(TItem item)
    {
      return Contains(item);
    }

    /// <inheritdoc/>
    bool IIndex<TKey, TItem>.ContainsKey(TKey key)
    {
      return ContainsKey(key);
    }

    #endregion

    #region IOrderedEnumerable<TKey,TItem> Members

    /// <inheritdoc/>
    SeekResult<TItem> IOrderedEnumerable<TKey, TItem>.Seek(Ray<Entire<TKey>> ray)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.Seek(ray);
    }

    /// <inheritdoc/>
    SeekResult<TItem> IOrderedEnumerable<TKey, TItem>.Seek(TKey key)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.Seek(key);
    }

    /// <inheritdoc/>
    IEnumerable<TKey> IOrderedEnumerable<TKey, TItem>.GetKeys(Range<Entire<TKey>> range)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.GetKeys(range);
    }

    /// <inheritdoc/>
    IIndexReader<TKey, TItem> IOrderedEnumerable<TKey, TItem>.CreateReader(Range<Entire<TKey>> range)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.CreateReader(range);
    }

    /// <inheritdoc/>
    IEnumerable<TItem> IOrderedEnumerable<TKey, TItem>.GetItems(Range<Entire<TKey>> range)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.GetItems(range);
    }

    /// <inheritdoc/>
    IEnumerable<TItem> IOrderedEnumerable<TKey, TItem>.GetItems(RangeSet<Entire<TKey>> range)
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.GetItems(range);
    }

    #endregion

    #region IEnumerable<TItem> Members

    /// <inheritdoc/>
    public new IEnumerator<TItem> GetEnumerator()
    {
      return index.GetEnumerator();
    }

    #endregion

    #region IOptimizationInfoProvider<TKey> Members

    /// <inheritdoc/>
    public IStatistics<TKey> GetStatistics()
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.GetStatistics();
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<TKey>> GetEntireKeyComparer()
    {
      if (orderedIndex == null)
        throw new NotSupportedException();
      return orderedIndex.GetEntireKeyComparer();
    }

    #endregion

    #region Private methods

    private TItem GetItem(TKey key)
    {
      return uniqueIndex.GetItem(key);
    }

    private void Add(TItem item)
    {
      index.Add(item);
    }

    private void Remove(TItem item)
    {
      if (nonUniqueIndex != null)
        nonUniqueIndex.Remove(item);
      else
        uniqueIndex.Remove(item);
    }

    private void Clear()
    {
      index.Clear();
    }

    private bool Contains(TItem item)
    {
      return index.Contains(item);
    }

    private bool ContainsKey(TKey key)
    {
      return index.ContainsKey(key);
    }

    private IEnumerable<TItem> GetItems(TKey key)
    {
      return nonUniqueIndex.GetItems(key);
    }

    #endregion

    #region OnXxx handlers

    private void OnCollectionCleared(object sender, ChangeNotifierEventArgs args)
    {
      Clear();
    }

    private void OnItemChanged(object sender, CollectionChangeNotifierEventArgs<TItem> args)
    {
      if (uniqueIndex != null && ContainsKey(index.KeyExtractor(args.Item))) {
        pendingItems.Remove(args.Item);
        throw new InvalidOperationException(Strings.ExUniqueConstraintViolation);
      }
      TKey key;
      if (pendingItems.TryGetValue(args.Item, out key))
        Remove(args.Item);
      pendingItems.Remove(args.Item);
      Add(args.Item);
    }

    private void OnItemChanging(object sender, CollectionChangeNotifierEventArgs<TItem> args)
    {
      pendingItems[args.Item] = index.KeyExtractor(args.Item);
    }

    private void OnItemInserted(object sender, CollectionChangeNotifierEventArgs<TItem> args)
    {
      Add(args.Item);
    }

    private void OnItemInserting(object sender, CollectionChangeNotifierEventArgs<TItem> args)
    {
      if (ContainsKey(index.KeyExtractor(args.Item)))
        throw new InvalidOperationException(Strings.ExUniqueConstraintViolation);
    }

    private void OnItemRemoved(object sender, CollectionChangeNotifierEventArgs<TItem> args)
    {
      Remove(args.Item);
    }

    #endregion

    /// <inheritdoc/>
    public TItem Resolve(TKey identifier)
    {
      return GetItem(identifier);
    }


    // Constructors

    private CollectionIndex(string name, ICollectionChangeNotifier<TItem> collection, IIndex<TKey, TItem> implementation,
                             bool isUnique)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(collection, "collection");
      ArgumentValidator.EnsureArgumentNotNull(implementation, "implementation");

      this.name = name;
      index = implementation;
      orderedIndex = implementation as IOrderedIndex<TKey, TItem>;
      if (isUnique)
        uniqueIndex = implementation as IUniqueIndex<TKey, TItem>;
      else
        nonUniqueIndex = implementation as INonUniqueIndex<TKey, TItem>;
      measurable = implementation as IMeasurable<TItem>;
      rangeMeasurable = measurable as IRangeMeasurable<TKey, TItem>;

      collection.Cleared += OnCollectionCleared;
      if (uniqueIndex != null)
        collection.Inserting += OnItemInserting;
      collection.Inserted += OnItemInserted;
      collection.Removed += OnItemRemoved;
      collection.ItemChanging += OnItemChanging;
      collection.ItemChanged += OnItemChanged;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Index name.</param>
    /// <param name="collection">The collection to bind the index to.</param>
    /// <param name="implementation">The index implementor.</param>
    public CollectionIndex(string name, ICollectionChangeNotifier<TItem> collection, IUniqueIndex<TKey, TItem> implementation) : 
      this(name, collection, implementation, true)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Index name.</param>
    /// <param name="collection">The collection to bind the index to.</param>
    /// <param name="implementation">The index implementor.</param>
    public CollectionIndex(string name, ICollectionChangeNotifier<TItem> collection, INonUniqueIndex<TKey, TItem> implementation) : 
      this(name, collection, implementation, false)
    {
    }
  }
}