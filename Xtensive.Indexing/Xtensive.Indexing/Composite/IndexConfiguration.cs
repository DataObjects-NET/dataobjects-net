// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Configuration;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// A <see cref="CompositeIndex{TKey,TItem}"/> configuration.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the Item.</typeparam>
  [Serializable]
  public class IndexConfiguration<TKey, TItem> : UniqueIndexWrapperConfiguration<TKey, TItem, TKey, TItem>
    where TKey : Tuple
    where TItem : Tuple
  {
    private IndexSegmentConfigurationSet<TKey, TItem> segments = new IndexSegmentConfigurationSet<TKey, TItem>();

    /// <summary>
    /// Gets the segment configurations.
    /// </summary>
    /// <value>The segment configurations.</value>
    public IndexSegmentConfigurationSet<TKey, TItem> Segments
    {
      [DebuggerStepThrough]
      get { return segments; }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      base.Validate();
      if (segments.Count==0)
        throw Exceptions.NotInitialized("Segments");
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexConfiguration<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      IndexConfiguration<TKey, TItem> indexConfiguration = (IndexConfiguration<TKey, TItem>) source;
      segments = (IndexSegmentConfigurationSet<TKey, TItem>) indexConfiguration.segments.Clone();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexConfiguration()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="uniqueIndexConfiguration">The unique index configuration.</param>
    public IndexConfiguration(IndexConfigurationBase<TKey, TItem> uniqueIndexConfiguration)
      : base(uniqueIndexConfiguration)
    {
    }
  }
}