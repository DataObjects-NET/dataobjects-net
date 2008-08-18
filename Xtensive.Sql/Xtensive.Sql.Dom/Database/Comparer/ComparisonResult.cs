// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for comparison results.
  /// </summary>
  public abstract class ComparisonResult : LockableBase
  {
    private readonly CollectionBaseSlim<PropertyComparisonResult> properties = new CollectionBaseSlim<PropertyComparisonResult>();
    private bool hasChanges;
    private ComparisonResultType result;

    /// <summary>
    /// Gets collection of property changes.
    /// </summary>
    public CollectionBaseSlim<PropertyComparisonResult> Properties
    {
      get { return properties; }
    }

    /// <summary>
    /// Gets <see langword="true"/> if result contains changes, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool HasChanges
    {
      get { return hasChanges; }
      internal set
      {
        this.EnsureNotLocked();
        hasChanges = value;
      }
    }

    /// <summary>
    /// Gets result type.
    /// </summary>
    public ComparisonResultType Result
    {
      get { return result; }
      internal set
      {
        this.EnsureNotLocked();
        result = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        properties.Lock(true);
      }
    }
  }
}