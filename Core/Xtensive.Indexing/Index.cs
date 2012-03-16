// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Diagnostics;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Providers;

namespace Xtensive.Indexing
{
  /// <summary>
  /// B+ index based index implementation.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public sealed partial class Index<TKey, TItem> :
    UniqueOrderedIndexBase<TKey, TItem>,
    IDisposable
  {
    private DescriptorPage<TKey, TItem> descriptorPage;
    private IPageRef cachedRootPageRef;
    private IPageRef cachedLeftmostPageRef;
    private IPageRef cachedRightmostPageRef;
    private int cachedPageSize = -1;


    /// <inheritdoc/>
    public override bool HasMeasures
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    #region Properties: Features, PageSize

    /// <summary>
    /// Gets index  features.
    /// Property can be set only once.
    /// </summary>
    public IndexFeatures Features
    {
      [DebuggerStepThrough]
      get { return provider.Features; }
    }

    /// <summary>
    /// Gets page size of the index.
    /// </summary>
    public int PageSize
    {
      [DebuggerStepThrough]
      get
      {
        if (cachedPageSize < 0)
          cachedPageSize = DescriptorPage.PageSize;
        return cachedPageSize;
      }
    }

    #endregion

    #region Shortcut properties: DescriptorPage, RootPage(Ref), LeftmostPage(Ref), RightmostPage(Ref)

    internal DescriptorPage<TKey, TItem> DescriptorPage
    {
      [DebuggerStepThrough]
      get { return descriptorPage; }
      [DebuggerStepThrough]
      set { descriptorPage = value; }
    }

    private IPageRef RootPageRef
    {
      [DebuggerStepThrough]
      get
      {
        if (cachedRootPageRef==null)
          cachedRootPageRef = DescriptorPage.RootPageRef;
        return cachedRootPageRef;
      }
      [DebuggerStepThrough]
      set
      {
        DescriptorPage.RootPageRef = value;
        cachedRootPageRef = value;
      }
    }

    internal DataPage<TKey, TItem> RootPage
    {
      [DebuggerStepThrough]
      get { return (DataPage<TKey, TItem>)provider.Resolve(RootPageRef); }
    }

    private IPageRef LeftmostPageRef
    {
      [DebuggerStepThrough]
      get
      {
        if (cachedLeftmostPageRef==null)
          cachedLeftmostPageRef = DescriptorPage.LeftmostPageRef;
        return cachedLeftmostPageRef;
      }
    }

    internal LeafPage<TKey, TItem> LeftmostPage
    {
      [DebuggerStepThrough]
      get { return (LeafPage<TKey, TItem>)provider.Resolve(LeftmostPageRef); }
    }

    private IPageRef RightmostPageRef
    {
      [DebuggerStepThrough]
      get
      {
        if (cachedRightmostPageRef==null)
          cachedRightmostPageRef = DescriptorPage.RightmostPageRef;
        return cachedRightmostPageRef;
      }
      [DebuggerStepThrough]
      set
      {
        DescriptorPage.RightmostPageRef = value;
        cachedRightmostPageRef = value;
      }
    }

    internal LeafPage<TKey, TItem> RightmostPage
    {
      [DebuggerStepThrough]
      get { return (LeafPage<TKey, TItem>)provider.Resolve(RightmostPageRef); }
    }

    #endregion

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      EnsureConfigured();
      key = ToFastKey(key);
      return InternalGetItem(RootPage, key);
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      EnsureConfigured();
      key = ToFastKey(key);
      if (DescriptorPage.BloomFilter!=null && !DescriptorPage.BloomFilter.HasValue(key))
        return false;
      return InternalContainsKey(RootPage, key);
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(TKey key)
    {
      EnsureConfigured();
      key = ToFastKey(key);
      TItem result;
      var seekResult = InternalSeek(RootPage, key);
      if (seekResult.ResultType == SeekResultType.Exact || seekResult.ResultType == SeekResultType.Nearest)
        result = seekResult.Pointer.Current;
      else
        result = default(TItem);
      return new SeekResult<TItem>(seekResult.ResultType, result);
    }


    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<Entire<TKey>> ray)
    {
      EnsureConfigured();
      ToFastRay(ref ray);
      TItem result;
      var seekResult = InternalSeek(RootPage, ray);
      if (seekResult.ResultType==SeekResultType.Exact || seekResult.ResultType==SeekResultType.Nearest)
        result = seekResult.Pointer.Current;
      else
        result = default(TItem);
      return new SeekResult<TItem>(seekResult.ResultType, result);
    }

    /// <inheritdoc/>
    public override IIndexReader<TKey, TItem> CreateReader(Range<Entire<TKey>> range)
    {
      EnsureConfigured();
      ToFastRange(ref range);
      return new IndexReader<TKey, TItem>(this, range);
    }

    #endregion

    /// <inheritdoc/>
    public override void Configure(IndexConfigurationBase<TKey, TItem> configuration)
    {
      var indexConfiguration = (IndexConfiguration<TKey, TItem>)configuration;
      if (indexConfiguration.Location==null)
        provider = new MemoryPageProvider<TKey, TItem>();
      else
        provider = new StreamPageProvider<TKey, TItem>(indexConfiguration.Location.Resource, indexConfiguration.CacheSize);
      provider.Index = this;
      descriptorPage = new DescriptorPage<TKey, TItem>(indexConfiguration, provider);
      descriptorPage.Initialize(); // Ensures it is created as well
      if (!provider.IsInitialized)
        provider.Initialize();
      base.Configure(configuration);
    }

    /// <inheritdoc/>
    protected override void ValidateConfiguration()
    {
      base.ValidateConfiguration();
      var configuration = (IndexConfiguration<TKey, TItem>)Configuration;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Index()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="configuration">Index configuration.</param>
    public Index(IndexConfiguration<TKey, TItem> configuration)
      : base(configuration)
    {
    }

    #region Dispose pattern (IDisposable, finalizer) implementation

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    private void Dispose(bool disposing)
    {
      provider.DisposeSafely();
      provider = null;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~Index()
    {
      Dispose(false);
    }

    #endregion
  }
}