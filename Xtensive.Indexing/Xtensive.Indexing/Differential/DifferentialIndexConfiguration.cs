// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.13

using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

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

    /// <summary>
    /// Gets or sets the origin configuration.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Origin
    {
      get { return origin; }
      set
      {
        this.EnsureNotLocked();
        origin = value;
      }
    }


    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new DifferentialIndexConfiguration<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      DifferentialIndexConfiguration<TKey, TItem> indexConfiguration = (DifferentialIndexConfiguration<TKey, TItem>)source;
//      insert = indexConfiguration.SegmentName;
    }

    #endregion


    // Constructors
    
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