// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.29

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Defines index configuration.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class IndexConfiguration<TKey, TItem> : IndexConfigurationBase<TKey, TItem>
  {
    /// <summary>
    /// Default order.
    /// </summary>
    public const int DefaultPageSize = 256;

    /// <summary>
    /// Default value for <see cref="BloomFilterBitsPerValue"/> property.
    /// </summary>
    public const double DefaultBloomFilterBitsPerValue = 16;

    /// <summary>
    /// Default value for <see cref="CacheSize"/> property.
    /// </summary>
    public const int DefaultCacheSize = 1024;

    private int pageSize;
    private bool useBloomFilter = true;
    private double bloomFilterBitsPerValue = DefaultBloomFilterBitsPerValue;
    private int cacheSize = DefaultCacheSize;


    /// <summary>
    /// Gets or sets number of pages cached by for page provider. 
    /// </summary>
    public int CacheSize
    {
      [DebuggerStepThrough]
      get { return cacheSize; }
      set
      {
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, int.MaxValue, "value");
        cacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets <see langword="true"/> if index uses <see cref="BloomFilter{T}"/> to optimize access. It is <see langword="true"/> by default.
    /// </summary>
    public bool UseBloomFilter
    {
      [DebuggerStepThrough]
      get { return useBloomFilter; }
      [DebuggerStepThrough]
      set { useBloomFilter = value; }
    }

    /// <summary>
    /// Gets or sets count of bits <see cref="BloomFilter{T}"/> uses per value. <see cref="DefaultBloomFilterBitsPerValue"/> is default value.
    /// </summary>
    public double BloomFilterBitsPerValue
    {
      [DebuggerStepThrough]
      get { return bloomFilterBitsPerValue; }
      set
      {
        ArgumentValidator.EnsureArgumentIsInRange<double>(value, 1, int.MaxValue, "value");
        bloomFilterBitsPerValue = value;
      }
    }

    /// <summary>
    /// Gets or sets index page size. It must be even value in range [2,65534].
    /// </summary>
    public int PageSize
    {
      [DebuggerStepThrough]
      get { return pageSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsNotDefault(value, "value");
        ArgumentValidator.EnsureArgumentIsInRange(value, 2, 65535, "value");
        if (value%2!=0) {
          throw new ArgumentException(Strings.ExInvalidPageSize);
        }
        pageSize = value;
      }
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexConfiguration<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      IndexConfiguration<TKey, TItem> configuration = (IndexConfiguration<TKey, TItem>)source;
      pageSize = configuration.pageSize;
      useBloomFilter = configuration.useBloomFilter;
      bloomFilterBitsPerValue = configuration.bloomFilterBitsPerValue;
    }

    #endregion

    // Constructors

    public IndexConfiguration()
    {
      pageSize = DefaultPageSize;
    }

    public IndexConfiguration(Converter<TItem, TKey> keyExtractor)
      : base(keyExtractor)
    {
      pageSize = DefaultPageSize;
    }

    public IndexConfiguration(Converter<TItem, TKey> keyExtractor, AdvancedComparer<TKey> keyComparer)
      : base(keyExtractor, keyComparer)
    {
      pageSize = DefaultPageSize;
    }
  }
}