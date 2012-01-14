// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.02

using System;
using System.Diagnostics;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// Base class for <see cref="InnerPage{TKey,TValue}"/>
  /// and <see cref="LeafPage{TKey,TValue}"/> of the
  /// <see cref="Index{TKey,TItem}"/>.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Node type.</typeparam>
  [Serializable]
  [DebuggerDisplay("Key = {Key}, CurrentSize = {CurrentSize} / {Size}")]
  public abstract class DataPage<TKey, TItem> : Page<TKey, TItem>,
    IHasMeasureResults<TItem>
  {
    private readonly IMeasureResultSet<TItem> measureResults;
    private int currentSize;

    /// <summary>
    /// Gets the key extractor (shortcut to <see cref="IndexBase{TKey,TItem}.KeyExtractor"/> property.
    /// </summary>
    /// <value>The key extractor.</value>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get { return Provider.Index.KeyExtractor; }
    }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    public abstract int Size { get; }

    /// <summary>
    /// Gets or sets the current size of the page.
    /// </summary>
    public int CurrentSize
    {
      [DebuggerStepThrough]
      get { return currentSize; }
      [DebuggerStepThrough]
      set { currentSize = value; }
    }

    /// <inheritdoc/>
    public IMeasureResultSet<TItem> MeasureResults
    {
      [DebuggerStepThrough]
      get { return measureResults; }
    }

    /// <summary>
    /// Gets key of the page.
    /// </summary>
    public abstract TKey Key { get; }

    /// <summary>
    /// Gets the key at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of the key to get.</param>
    public abstract TKey GetKey(int index);

    #region Seek methods

    /// <summary>
    /// Seeks for the specified key.
    /// </summary>
    /// <param name="key">The key to seek for.</param>
    /// <returns>Standard seek operation result.</returns>
    public abstract SeekResultPointer<int> Seek(TKey key);

    /// <summary>
    /// Seeks for the specified ray.
    /// </summary>
    /// <param name="ray">The ray to seek for.</param>
    /// <returns>Standard seek operation result.</returns>
    public abstract SeekResultPointer<int> Seek(Ray<Entire<TKey>> ray);

    #endregion

    #region Remove, Split, Merge methods

    /// <summary>
    /// Removes the key at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of the key to remove.</param>
    public abstract void Remove(int index);

    /// <summary>
    /// Splits the page into two pages.
    /// </summary>
    /// <returns>The page created during the split operation.</returns>
    public abstract DataPage<TKey, TItem> Split();

    /// <summary>
    /// Merges the page with the specified one.
    /// </summary>
    /// <param name="page">The page to merge with.</param>
    /// <returns><see langword="True"/> if pages were merged into a single one;
    /// otherwise (i.e. if second page still contains the items), <see langword="false"/>.</returns>
    public abstract bool Merge(DataPage<TKey, TItem> page);

    #endregion

    #region Measure related methods (all are abstract)

    /// <summary>
    /// Recalculates the all measures.
    /// </summary>
    public abstract void RecalculateMeasures();

    /// <summary>
    /// Adds specified <paramref name="item"/> to all the measures.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public abstract void AddToMeasures(TItem item);

    /// <summary>
    /// Adds all the measure results of specified <paramref name="hasMeasures"/> to all the measures.
    /// </summary>
    /// <param name="hasMeasures">The measure results owner.</param>
    public abstract void AddToMeasures(IHasMeasureResults<TItem> hasMeasures);

    /// <summary>
    /// Subtracts specified <paramref name="item"/> from all the measures.
    /// </summary>
    /// <param name="item">The item to subtract.</param>
    public abstract void SubtractFromMeasures(TItem item);

    /// <summary>
    /// Subtracts all the measure results of specified <paramref name="hasMeasures"/> from all the measures.
    /// </summary>
    /// <param name="hasMeasures">The measure results owner.</param>
    public abstract void SubtractFromMeasures(IHasMeasureResults<TItem> hasMeasures);

    #endregion

    
    // Constructors

    /// <inheritdoc/>
    protected DataPage(IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
      if (provider.Index.HasMeasures)
        measureResults = new MeasureResultSet<TItem>(provider.Index.DescriptorPage.Measures);
    }
  }
}