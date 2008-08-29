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
  public abstract class ComparisonResult : LockableBase, 
    IComparisonResult
  {
    private readonly Type type;
    private ComparisonResultType resultType;

    /// <summary>
    /// Gets <see langword="true"/> if result contains changes, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool HasChanges
    {
      get { return resultType!=ComparisonResultType.Unchanged; }
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

    public ComparisonResult(Type type)
    {
      this.type = type;
    }
  }
}