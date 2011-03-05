// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.20

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Configuration;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base class for all indexes.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public abstract class IndexBase<TKey, TItem>: ConfigurableBase<IndexConfigurationBase<TKey, TItem>>,
    IIndex<TKey, TItem>
  {
    private Func<Entire<TKey>, TKey, int> asymmetricKeyCompare;
    private AdvancedComparer<Entire<TKey>> entireKeyComparer;
    private AdvancedComparer<TKey> keyComparer;
    private Converter<TItem, TKey> keyExtractor;
    private IMeasureSet<TItem> measures;

    #region Properties: comparers, key extractor

    /// <inheritdoc/>
    public AdvancedComparer<TKey> KeyComparer
    {
      get { return keyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<TKey>> EntireKeyComparer
    {
      get { return entireKeyComparer; }
    }

    /// <inheritdoc/>
    public Func<Entire<TKey>, TKey, int> AsymmetricKeyCompare
    {
      get { return asymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    #endregion

    /// <inheritdoc/>
    public virtual long Count
    {
      [DebuggerStepThrough]
      get { return (long) GetMeasureResult(CountMeasure<object, long>.CommonName); }
    }

    /// <inheritdoc/>
    public virtual long Size
    {
      [DebuggerStepThrough]
      get { return (long) GetMeasureResult(SizeMeasure<object>.CommonName); }
    }

    /// <inheritdoc/>
    public virtual bool HasMeasures
    {
      [DebuggerStepThrough]
      get { return measures != null && measures.Count > 0; }
    }

    /// <inheritdoc/>
    public virtual IMeasureSet<TItem> Measures
    {
      [DebuggerStepThrough]
      get { return measures; }
    }

    #region Contains, ContainsKey methods

    /// <inheritdoc/>
    public abstract bool Contains(TItem item);

    /// <inheritdoc/>
    public abstract bool ContainsKey(TKey key);

    #endregion

    #region Modification methods - Add, Remove, etc.

    /// <inheritdoc/>
    public abstract void Add(TItem item);

    /// <inheritdoc/>
    public abstract bool Remove(TItem item);

    /// <inheritdoc/>
    public abstract void Replace(TItem item);

    /// <inheritdoc/>
    public abstract bool RemoveKey(TKey key);

    /// <inheritdoc/>
    public abstract void Clear();

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public abstract IEnumerator<TItem> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public abstract object GetMeasureResult(string name);

    /// <inheritdoc/>
    public abstract object[] GetMeasureResults(params string[] names);

    #endregion

    protected override void OnConfigured()
    {
      base.OnConfigured();
      keyExtractor = Configuration.KeyExtractor;
      keyComparer = Configuration.KeyComparer;
      entireKeyComparer = Configuration.EntireKeyComparer;
      asymmetricKeyCompare = Configuration.AsymmetricKeyCompare;
      measures = (IMeasureSet<TItem>)Configuration.Measures.Clone();
    }

    
    // Constructors

    /// <inheritdoc/>
    protected IndexBase()
    {
    }

    /// <inheritdoc/>
    protected IndexBase(IndexConfigurationBase<TKey, TItem> configuration)
      : base(configuration)
    {
    }
  }
}