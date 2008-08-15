// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for compare results.
  /// </summary>
  public abstract class CompareResult : LockableBase
  {
    private readonly CollectionBaseSlim<PropertyCompareResult> properties = new CollectionBaseSlim<PropertyCompareResult>();
    private bool hasChanges;
    private CompareResultType result;

    /// <summary>
    /// Gets collection of property changes.
    /// </summary>
    public CollectionBaseSlim<PropertyCompareResult> Properties
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
    public CompareResultType Result
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