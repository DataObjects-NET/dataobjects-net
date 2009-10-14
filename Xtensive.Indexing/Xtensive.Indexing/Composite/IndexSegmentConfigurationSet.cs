// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using Xtensive.Core.Configuration;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// A set of <see cref="IndexSegmentConfiguration{TKey,TItem}"/> items.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <seealso cref="IndexConfigurationBase{TKey,TITem}"/>
  [Serializable]
  public class IndexSegmentConfigurationSet<TKey, TItem> : ConfigurationSetBase<IndexSegmentConfiguration<TKey, TItem>>
    where TKey : Tuple 
    where TItem : Tuple
  {
    /// <inheritdoc/>
    protected override string GetItemName(IndexSegmentConfiguration<TKey, TItem> item)
    {
      return item.SegmentName;
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexSegmentConfigurationSet<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      IndexSegmentConfigurationSet<TKey, TItem> set = (IndexSegmentConfigurationSet<TKey, TItem>)source;
      foreach (IndexSegmentConfiguration<TKey, TItem> configuration in set)
        Add((IndexSegmentConfiguration<TKey, TItem>)configuration.Clone());
    }
  }
}