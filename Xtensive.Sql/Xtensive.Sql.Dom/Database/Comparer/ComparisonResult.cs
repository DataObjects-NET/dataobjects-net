// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for comparison results.
  /// </summary>
  public abstract class ComparisonResult : LockableBase
  {
    private Type type;
    private bool hasChanges;
    private ComparisonResultType comparisonType;
    private readonly ComparisonResultCollection<ComparisonResult> nested = new ComparisonResultCollection<ComparisonResult>();
    private readonly ComparisonResultCollection<PropertyComparisonResult> properties = new ComparisonResultCollection<PropertyComparisonResult>();

    /// <summary>
    /// Gets comparison result of nested nodes.
    /// </summary>
    public ICollection<ComparisonResult> Nested
    {
      get { return nested; }
    }

    /// <summary>
    /// Gets comparison result of node properties.
    /// </summary>
    public ICollection<PropertyComparisonResult> Properties
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

    public IEnumerable<ComparisonResult> Find(ComparisonResultLocation locations, ComparisonResultType comparisonTypes, bool recursive, params Type[] types)
    {
      IEnumerable<ComparisonResult> propertyResults = properties.Find(locations, comparisonTypes, recursive, types);
      IEnumerable<ComparisonResult> nestedResults = nested.Find(locations, comparisonTypes, recursive, types);
      return propertyResults.Union(nestedResults);
    }

    /// <summary>
    /// Gets comparison type.
    /// </summary>
    public ComparisonResultType ComparisonType
    {
      get { return comparisonType; }
      internal set
      {
        this.EnsureNotLocked();
        comparisonType = value;
      }
    }

    public Type Type
    {
      get { return type; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        nested.Lock(true);
        properties.Lock(true);
      }
    }

    public ComparisonResult(Type type)
    {
      this.type = type;
    }

  }
}