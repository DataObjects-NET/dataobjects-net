// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using System.Diagnostics;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// The configuration of <see cref="IOrderedIndex{TKey,TItem}"/> wrapper.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <typeparam name="TUniqueKey">The type of the unique key.</typeparam>
  /// <typeparam name="TUniqueItem">The type of the unique item.</typeparam>
  public abstract class UniqueIndexWrapperConfiguration<TKey, TItem, TUniqueKey, TUniqueItem>: IndexConfigurationBase<TKey, TItem>
  {
    private IUniqueOrderedIndex<TUniqueKey, TUniqueItem> uniqueIndex;
    private IndexConfigurationBase<TUniqueKey, TUniqueItem> uniqueIndexConfiguration;

    /// <summary>
    /// Gets or sets the configuration of the unique index.
    /// </summary>
    /// <value>The base index configuration.</value>
    public IndexConfigurationBase<TUniqueKey, TUniqueItem> UniqueIndexConfiguration
    {
      [DebuggerStepThrough]
      get { return uniqueIndexConfiguration; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        uniqueIndexConfiguration = value;
      }
    }

    /// <summary>
    /// Gets or sets the unique index.
    /// </summary>
    /// <value>The unique index.</value>
    public IUniqueOrderedIndex<TUniqueKey, TUniqueItem> UniqueIndex
    {
      [DebuggerStepThrough]
      get { return uniqueIndex; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        uniqueIndex = value;
      }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      base.Validate();
      if (uniqueIndexConfiguration == null)
        throw Exceptions.NotInitialized("UniqueIndexConfiguration");
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      UniqueIndexWrapperConfiguration<TKey, TItem, TUniqueKey, TUniqueItem> indexConfiguration =
        (UniqueIndexWrapperConfiguration<TKey, TItem, TUniqueKey, TUniqueItem>)source;
      uniqueIndexConfiguration = indexConfiguration.uniqueIndexConfiguration;
      uniqueIndex = indexConfiguration.uniqueIndex;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive && uniqueIndexConfiguration != null)
        uniqueIndexConfiguration.Lock(recursive);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected UniqueIndexWrapperConfiguration() : this(new IndexConfigurationBase<TUniqueKey, TUniqueItem>())
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="uniqueIndexConfiguration">The unique index configuration.</param>
    protected UniqueIndexWrapperConfiguration(IndexConfigurationBase<TUniqueKey, TUniqueItem> uniqueIndexConfiguration)
    {
      UniqueIndexConfiguration = uniqueIndexConfiguration;
    }
  }
}