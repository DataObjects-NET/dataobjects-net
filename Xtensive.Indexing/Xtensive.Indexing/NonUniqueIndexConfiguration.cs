// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.22

using System;
using System.Diagnostics;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Index configuration. 
  /// Used by <see cref="NonUniqueIndex{TKey,TUniqueKey,TItem}"/> 
  /// to unify index initialization and configuration.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TUniqueKey">The type of unique index key.</typeparam>
  /// <typeparam name="TItem">The type of the index item.</typeparam>
  [Serializable]
  public class NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem>: UniqueIndexWrapperConfiguration<TKey, TItem, TUniqueKey, TItem> 
  {
    private Converter<Entire<TKey>, Entire<TUniqueKey>> entireConverter;

    /// <summary>
    /// Gets or sets the key converter.
    /// </summary>
    /// <value>The key converter.</value>
    public Converter<Entire<TKey>, Entire<TUniqueKey>> EntireConverter
    {
      [DebuggerStepThrough]
      get { return entireConverter; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        entireConverter = value;
      }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      base.Validate();
      if (entireConverter==null)
        throw Exceptions.NotInitialized("KeyConverter");
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem>();
    }

    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem> indexConfiguration = (NonUniqueIndexConfiguration<TKey,TUniqueKey,TItem>)source;
      entireConverter = indexConfiguration.entireConverter;
    }


    // Constructors

    /// <inheritdoc/>
    public NonUniqueIndexConfiguration()
    {
    }

    /// <inheritdoc/>
    public NonUniqueIndexConfiguration(IndexConfigurationBase<TUniqueKey, TItem> uniqueIndexConfiguration)
      : base(uniqueIndexConfiguration)
    {
    }
  }
}