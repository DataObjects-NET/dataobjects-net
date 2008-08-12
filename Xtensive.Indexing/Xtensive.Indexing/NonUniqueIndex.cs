// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Implements <see cref="INonUniqueIndex{TKey,TItem}"/> wrapper over <see cref="IUniqueIndex{TKey,TItem}"/> instance.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TUniqueKey">The type of the unique key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class NonUniqueIndex<TKey, TUniqueKey, TItem>: IndexBase<TKey, TItem>,
    INonUniqueIndex<TKey, TItem>
  {
    private IUniqueOrderedIndex<TUniqueKey, TItem> uniqueIndex;
    private Converter<IEntire<TKey>, IEntire<TUniqueKey>> entireConverter;

    /// <summary>
    /// Gets the underlying unique index.
    /// </summary>
    protected internal IUniqueOrderedIndex<TUniqueKey, TItem> UniqueIndex
    {
      [DebuggerStepThrough]
      get { return uniqueIndex; }
    }

    /// <summary>
    /// Gets the entire converter.
    /// </summary>
    /// <value>The key converter.</value>
    protected internal Converter<IEntire<TKey>, IEntire<TUniqueKey>> EntireConverter
    {
      [DebuggerStepThrough]
      get { return entireConverter; }
    }

    /// <inheritdoc/>
    public override long Count
    {
      [DebuggerStepThrough]
      get { return uniqueIndex.Count; }
    }

    #region Contains, ContainsKey methods

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return uniqueIndex.Contains(item);
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return GetItems(key).GetEnumerator().MoveNext();
    }

    #endregion

    #region GetKeys, GetItems methods

    /// <inheritdoc/>
    public IEnumerable<TKey> GetKeys(Range<IEntire<TKey>> range)
    {
      TKey previousKey = default(TKey);
      bool bFirst = true;
      foreach (TItem item in GetItems(range)) {
        var key = KeyExtractor(item);
        if (bFirst) {
          previousKey = key;
          bFirst = false;
          yield return key;
        }
        else {
          if (!KeyComparer.Equals(previousKey, key)) {
            previousKey = key;
            yield return key;
          }
        }
      }
    }

    /// <inheritdoc/>
    public IEnumerable<TItem> GetItems(TKey key)
    {
      var keyRange = new Range<IEntire<TKey>>(
        Entire<TKey>.Create(key, Direction.Negative),
        Entire<TKey>.Create(key, Direction.Positive))
        .Redirect(Direction.Positive, EntireKeyComparer);
      return GetItems(keyRange);
    }

    /// <inheritdoc/>
    public IEnumerable<TItem> GetItems(Range<IEntire<TKey>> range)
    {
      return CreateReader(range);
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public SeekResult<TItem> Seek(Ray<IEntire<TKey>> ray)
    {
      return uniqueIndex.Seek(GetUniqueIndexRay(ray));
    }

    /// <inheritdoc/>
    public IIndexReader<TKey, TItem> CreateReader(Range<IEntire<TKey>> range)
    {
      return new NonUniqueIndexReader<TKey, TUniqueKey, TItem>(this, range);
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public override void Add(TItem item)
    {
      uniqueIndex.Add(item);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      return uniqueIndex.Remove(item);
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      bool result = false;
      foreach (TItem item in GetItems(key)) {
        Remove(item);
        result = true;
      }
      return result;
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      uniqueIndex.Replace(item);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      uniqueIndex.Clear();
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      return uniqueIndex.GetMeasureResult(name);
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      return uniqueIndex.GetMeasureResults(names);
    }

    /// <inheritdoc/>
    public object GetMeasureResult(Range<IEntire<TKey>> range, string name)
    {
      return uniqueIndex.GetMeasureResult(GetUniqueIndexRange(range), name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names)
    {
      return uniqueIndex.GetMeasureResults(GetUniqueIndexRange(range), names);
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public override IEnumerator<TItem> GetEnumerator()
    {
      return CreateReader(this.GetFullRange());
    }

    #endregion

    #region Private \ internal methods

    internal Ray<IEntire<TUniqueKey>> GetUniqueIndexRay(Ray<IEntire<TKey>> ray)
    {
      IEntire<TUniqueKey> point = EntireConverter(ray.Point);
      return new Ray<IEntire<TUniqueKey>>(point, ray.Direction);
    }

    internal Range<IEntire<TUniqueKey>> GetUniqueIndexRange(Range<IEntire<TKey>> range)
    {
      IEntire<TUniqueKey> firstPoint  = EntireConverter(range.EndPoints.First);
      IEntire<TUniqueKey> secondPoint = EntireConverter(range.EndPoints.Second);
      return new Range<IEntire<TUniqueKey>>(new Pair<IEntire<TUniqueKey>>(firstPoint, secondPoint));
    }

    #endregion


    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      var configuration = (NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem>)Configuration;
      uniqueIndex = configuration.UniqueIndex;
      entireConverter = configuration.EntireConverter;
    }


    // Constructors

    /// <inheritdoc/>
    public NonUniqueIndex()
    {
    }

    /// <inheritdoc/>
    public NonUniqueIndex(IndexConfigurationBase<TKey, TItem> configuration)
      : base(configuration)
    {
    }
  }
}
