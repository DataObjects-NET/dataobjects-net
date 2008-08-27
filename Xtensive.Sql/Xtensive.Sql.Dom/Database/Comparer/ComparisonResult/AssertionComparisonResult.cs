// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Assertion"/> comparison result.
  /// </summary>
  [Serializable]
  public class AssertionComparisonResult : ComparisonResult<Assertion>
  {
    private ComparisonResult<SqlExpression> condition;
    private ComparisonResult<bool?> isDeferrable;
    private ComparisonResult<bool?> isInitiallyDeferred;

    /// <summary>
    /// Gets <see cref="Assertion.Condition"/> comparison result.
    /// </summary>
    public ComparisonResult<SqlExpression> Condition
    {
      get { return condition; }
      internal set
      {
        this.EnsureNotLocked();
        condition = value;
      }
    }

    /// <summary>
    /// Gets <see cref="Assertion.IsDeferrable"/> comparison result.
    /// </summary>
    public ComparisonResult<bool?> IsDeferrable
    {
      get { return isDeferrable; }
      internal set
      {
        this.EnsureNotLocked();
        isDeferrable = value;
      }
    }

    /// <summary>
    /// Gets <see cref="Assertion.IsInitiallyDeferred"/> comparison result.
    /// </summary>
    public ComparisonResult<bool?> IsInitiallyDeferred
    {
      get { return isInitiallyDeferred; }
      internal set
      {
        this.EnsureNotLocked();
        isInitiallyDeferred = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        condition.LockSafely(recursive);
        isDeferrable.LockSafely(recursive);
        isInitiallyDeferred.LockSafely(recursive);
      }
    }
  }
}