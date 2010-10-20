// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// The configuration of index segment.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the indexed item.</typeparam>
  /// <seealso cref="IndexConfigurationBase{TKey,TITem}"/>
  /// <seealso cref="CompositeIndex{TKey,TItem}"/>
  [Serializable]
  public class IndexSegmentConfiguration<TKey, TItem> : IndexConfigurationBase<TKey, TItem>
    where TKey : Tuple
    where TItem : Tuple
  {
    private string segmentName;
    private int segmentNumber;
    private readonly Dictionary<string, string> measureMapping = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the name of the segment.
    /// </summary>
    /// <value>The name of the segment.</value>
    public string SegmentName
    {
      [DebuggerStepThrough]
      get { return segmentName; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        segmentName = value;
      }
    }

    /// <summary>
    /// Gets the segment number.
    /// </summary>
    /// <value>The segment number.</value>
    public int SegmentNumber
    {
      [DebuggerStepThrough]
      get { return segmentNumber; }
      set
      {
        this.EnsureNotLocked();
        segmentNumber = value;
      }
    }

    /// <summary>
    /// Gets the measure mapping.
    /// </summary>
    /// <value>The measure mapping.</value>
    public Dictionary<string, string> MeasureMapping
    {
      [DebuggerStepThrough]
      get { return measureMapping; }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      base.Validate();
      if (String.IsNullOrEmpty(segmentName))
        throw Exceptions.NotInitialized("SegmentName");
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexSegmentConfiguration<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      IndexSegmentConfiguration<TKey, TItem> indexConfiguration = (IndexSegmentConfiguration<TKey, TItem>) source;
      segmentName = indexConfiguration.SegmentName;
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="segmentName">The name of the segment.</param>
    public IndexSegmentConfiguration(string segmentName)
    {
      this.segmentName = segmentName;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexSegmentConfiguration()
    {
    }
  }
}