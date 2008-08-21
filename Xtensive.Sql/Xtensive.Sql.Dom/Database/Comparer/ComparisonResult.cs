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
    private readonly Type type;
    private string name;
    private bool hasChanges;
    private ComparisonResultType resultType;

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
    /// Gets comparison node name.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        name = value;
      }
    }
//
//    public IEnumerable<ComparisonResult> Find(ComparisonResultLocation locations, ComparisonResultType comparisonTypes, bool recursive, params Type[] types)
//    {
//      IEnumerable<ComparisonResult> propertyResults = properties.Find(locations, comparisonTypes, recursive, types);
//      IEnumerable<ComparisonResult> nestedResults = nested.Find(locations, comparisonTypes, recursive, types);
//      return propertyResults.Union(nestedResults);
//    }

    /// <summary>
    /// Gets comparison type.
    /// </summary>
    public ComparisonResultType ResultType
    {
      get { return resultType; }
      internal set
      {
        this.EnsureNotLocked();
        resultType = value;
      }
    }

    public Type Type
    {
      get { return type; }
    }

//    /// <inheritdoc/>
//    public override void Lock(bool recursive)
//    {
//      base.Lock(recursive);
//      if (recursive) {
//        nested.Lock(true);
//        properties.Lock(true);
//      }
//    }

    public ComparisonResult(Type type)
    {
      this.type = type;
    }
  }
}