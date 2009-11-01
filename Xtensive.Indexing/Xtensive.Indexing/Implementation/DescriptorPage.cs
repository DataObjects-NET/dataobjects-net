// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.02

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Implementation
{
  [Serializable]
  public class DescriptorPage<TKey, TItem> : Page<TKey, TItem>
  {
    [NonSerialized] 
    private int pageSize;

    private IndexConfiguration<TKey, TItem> configuration;
    private IPageRef rootPageRef;
    private IPageRef leftmostPageRef;
    private IPageRef rightmostPageRef;
    private IMeasureSet<TItem> measures;
    [NonSerialized]
    private IBloomFilter<TKey> bloomFilter;

    public IndexConfiguration<TKey, TItem> Configuration
    {
      get { return configuration; }
    }

    public int PageSize
    {
      get { return pageSize; }
    }

    /// <summary>
    /// Gets the set of measures stored on this page.
    /// </summary>
    public IMeasureSet<TItem> Measures
    {
      get { return measures; }
    }

    /// <summary>
    /// Gets or sets the <see cref="IBloomFilter{T}"/> associated with the index.
    /// </summary>
    public IBloomFilter<TKey> BloomFilter
    {
      get { return bloomFilter; }
      set { bloomFilter = value; }
    }

    #region (Root\Leftmost\Rightmost)Page & PageRef

    public IPageRef RootPageRef
    {
      get { return rootPageRef; }
      set { rootPageRef = value; }
    }

    public DataPage<TKey, TItem> RootPage
    {
      get { return (DataPage<TKey, TItem>)Provider.Resolve(rootPageRef); }
      set { RootPageRef = value.Identifier; }
    }

    public IPageRef LeftmostPageRef
    {
      get { return leftmostPageRef; }
      set { leftmostPageRef = value; }
    }

    public LeafPage<TKey, TItem> LeftmostPage
    {
      get { return (LeafPage<TKey, TItem>)Provider.Resolve(leftmostPageRef); }
      set { LeftmostPageRef = value.Identifier; }
    }

    public IPageRef RightmostPageRef
    {
      get { return rightmostPageRef; }
      set { rightmostPageRef = value; }
    }

    public LeafPage<TKey, TItem> RightmostPage
    {
      get { return (LeafPage<TKey, TItem>)Provider.Resolve(rightmostPageRef); }
      set { RightmostPageRef = value.Identifier; }
    }

    #endregion

    /// <summary>
    /// Adds a <paramref name="measure"/> to the index.
    /// </summary>
    /// <param name="measure">Measure to add.</param>
    public void AddMeasure(IMeasure<TItem> measure)
    {
      ArgumentValidator.EnsureArgumentNotNull(measure, "measure");
      if (Provider.IsInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      EnsureMeasuresInitialized();
      try {
        measures.Add(measure);
      }
      finally {
        if (measures.Count==0)
          measures = null;
      }
    }

    /// <summary>
    /// Called to clear the index.
    /// </summary>
    public virtual void Clear()
    {
      LeafPage<TKey, TItem> firstPage = new LeafPage<TKey, TItem>(Provider);
      rootPageRef = leftmostPageRef = rightmostPageRef = firstPage.Identifier;
    }

    /// <summary>
    /// Initializes the descriptor page.
    /// Invoked e.g. by <see cref="Clear"/> method.
    /// </summary>
    public virtual void Initialize()
    {
      if (Provider.IsInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      Clear();
    }

    #region Private \ internal methods

    private void EnsureMeasuresInitialized()
    {
      if (measures==null)
        measures = new MeasureSet<TItem>();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor"/>
    /// </summary>
    /// <param name="configuration">Index configuration.</param>
    /// <param name="provider">Index page provider this page is bound to.</param>
    public DescriptorPage(IndexConfiguration<TKey, TItem> configuration, IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
      this.configuration = configuration;
      pageSize = configuration.PageSize;
      bool hasCountMeasure = false;
      for (int i = 0; i < configuration.Measures.Count; i++) {
        if (configuration.Measures[i] is CountMeasure<TItem, long>) {
          hasCountMeasure = true;
        }
        AddMeasure(configuration.Measures[i]);
      }
      if (!hasCountMeasure) {
        AddMeasure(new CountMeasure<TItem, long>());
      }
    }
  }
}