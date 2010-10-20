// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.13

using System;
using System.Diagnostics;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Differential
{
  /// <summary>
  /// Differential index configuration.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class DifferentialIndexConfiguration<TKey, TItem> : IndexConfigurationBase<TKey, TItem>
  {
    private IUniqueOrderedIndex<TKey, TItem> origin;
    private IUniqueOrderedIndex<TKey, TItem> insertions;
    private IUniqueOrderedIndex<TKey, TItem> removals;


    /// <summary>
    /// Gets or sets the origin configuration.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Origin
    {
      [DebuggerStepThrough]
      get { return origin; }
      set
      {
        this.EnsureNotLocked();
        origin = value;
      }
    }

    /// <summary>
    /// Gets or sets the origin configuration.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Insertions
    {
      [DebuggerStepThrough]
      get { return insertions; }
      set
      {
        this.EnsureNotLocked();
        insertions = value;
      }
    }

    /// <summary>
    /// Gets or sets the origin configuration.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Removals
    {
      [DebuggerStepThrough]
      get { return removals; }
      set
      {
        this.EnsureNotLocked();
        removals = value;
      }
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new DifferentialIndexConfiguration<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      DifferentialIndexConfiguration<TKey, TItem> indexConfiguration = (DifferentialIndexConfiguration<TKey, TItem>) source;
      origin = indexConfiguration.Origin;
      insertions = indexConfiguration.Insertions;
      removals = indexConfiguration.Removals;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DifferentialIndexConfiguration&lt;TKey, TItem&gt;"/> class.
    /// </summary>
    public DifferentialIndexConfiguration(IUniqueOrderedIndex<TKey, TItem> origin, IUniqueOrderedIndex<TKey, TItem> insertions, IUniqueOrderedIndex<TKey, TItem> removals)
    {
      this.origin = origin;
      this.insertions = insertions;
      this.removals = removals;
      KeyExtractor = origin.KeyExtractor;
      KeyComparer = origin.KeyComparer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DifferentialIndexConfiguration&lt;TKey, TItem&gt;"/> class.
    /// </summary>
    public DifferentialIndexConfiguration(IUniqueOrderedIndex<TKey, TItem> origin)
    {
      this.origin = origin;
      KeyExtractor = origin.KeyExtractor;
      KeyComparer = origin.KeyComparer;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DifferentialIndexConfiguration()
    {
    }
  }
}